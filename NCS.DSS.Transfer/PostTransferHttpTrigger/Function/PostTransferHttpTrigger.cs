using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.PostTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace NCS.DSS.Transfer.PostTransferHttpTrigger.Function
{
    public class PostTransferHttpTrigger
    {
        private readonly IPostTransferHttpTriggerService _transferPostService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ILogger<PostTransferHttpTrigger> _logger;
        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public PostTransferHttpTrigger(
            IPostTransferHttpTriggerService transferPostService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            IValidate validate,
            IDynamicHelper dynamicHelper,
            ILogger<PostTransferHttpTrigger> logger)
        {
            _transferPostService = transferPostService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
            _logger = logger;
        }

        [Function("POST")]
        [ProducesResponseType(typeof(Models.Transfer), (int)HttpStatusCode.Created)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Transfer Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Transfer does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "Transfer validation error(s)", ShowSchema = false)]
        [Display(Name = "POST", Description = "Ability to create a new transfer resource.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/")] HttpRequest req, string customerId, string interactionId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PostTransferHttpTrigger));

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("Unable to locate 'TouchpointId' in request header");
                return new BadRequestResult();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _logger.LogWarning("Unable to locate 'apimURL' in request header");
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to locate 'CustomerId' in request header");
                return new BadRequestObjectResult(customerGuid.ToString());
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _logger.LogWarning("Unable to locate 'InteractionId' in request header");
                return new BadRequestObjectResult(interactionGuid.ToString());
            }

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}", touchpointId);

            Models.Transfer transferRequest;

            try
            {
                _logger.LogInformation("Attempting to retrieve resource from request body");
                transferRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Transfer>(req);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unable to parse {TransferRequest} from request body. Exception: {ExceptionMessage}", nameof(transferRequest), ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
            }

            if (transferRequest == null)
            {
                _logger.LogInformation("{TransferRequest} object is NULL", nameof(transferRequest));
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation("Retrieved resource from request body");

            transferRequest.SetIds(customerGuid, interactionGuid, touchpointId);

            _logger.LogInformation("Attempting to validate {TransferRequest} object", nameof(transferRequest));
            var errors = _validate.ValidateResource(transferRequest, true);

            if (errors != null && errors.Any())
            {
                _logger.LogWarning("Falied to validate {TransferRequest}", nameof(transferRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Successfully validated {TransferRequest}", nameof(transferRequest));

            _logger.LogInformation("Attempting to check if customer exists. Customer GUID: {CustomerId}", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer does not exist. Customer GUID: {CustomerGuid}", customerGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}", customerGuid);

            _logger.LogInformation("Attempting to check if customer is read only. Customer GUID: {CustomerGuid}", customerGuid);
            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogWarning("Customer is read-only. Operation is forbidden. Customer GUID: {CustomerGuid}", customerGuid);
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}", customerGuid);

            _logger.LogInformation("Attempting to check Interaction exists for customer. Customer GUID: {CustomerGuid}. Interaction GUID: {InteractionGuid}", customerGuid, interactionGuid);
            var doesInteractionExist = await _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _logger.LogWarning("Interaction does not exist for customer. Customer GUID: {CustomerGuid}. Interaction GUID: {InteractionGuid}", customerGuid, interactionGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Interaction exists for customer. Customer GUID: {CustomerGuid}. Interaction GUID: {InteractionGuid}", customerGuid, interactionGuid);

            _logger.LogInformation("Attempting to POST a Transfer. Customer GUID: {CustomerGuid}", customerGuid);
            var transfer = await _transferPostService.CreateAsync(transferRequest);

            if (transfer != null)
            {
                _logger.LogInformation("Sending newly created Transfer to service bus. Customer GUID: {CustomerGuid}. Transfer ID: {TransferId}", customerGuid, transfer.TransferId.GetValueOrDefault());
                await _transferPostService.SendToServiceBusQueueAsync(transfer, ApimURL);
            }

            if (transfer == null)
            {
                _logger.LogWarning("PATCH request unsuccessful. Customer GUID: {CustomerGuid}", customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostTransferHttpTrigger));
                return new BadRequestObjectResult(customerGuid.ToString());
            }

            _logger.LogInformation("POST request successful. Transfer ID: {TransferId}", transfer.TransferId.GetValueOrDefault());
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostTransferHttpTrigger));
            return new JsonResult(transfer, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}