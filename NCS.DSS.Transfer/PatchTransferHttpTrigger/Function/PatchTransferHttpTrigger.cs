using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.PatchTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace NCS.DSS.Transfer.PatchTransferHttpTrigger.Function
{
    public class PatchTransferHttpTrigger
    {
        private readonly IPatchTransferHttpTriggerService _transferPatchService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ILogger<PatchTransferHttpTrigger> _logger;
        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public PatchTransferHttpTrigger(
            IPatchTransferHttpTriggerService transferPatchService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            IValidate validate,
            IDynamicHelper dynamicHelper,
            ILogger<PatchTransferHttpTrigger> logger)
        {
            _transferPatchService = transferPatchService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
            _logger = logger;
        }

        [Function("PATCH")]
        [ProducesResponseType(typeof(Models.Transfer), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Transfer Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Transfer does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "Transfer validation error(s)", ShowSchema = false)]
        [Display(Name = "PATCH", Description = "Ability to modify/update an transfer record.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")] HttpRequest req, string customerId, string interactionId, string transferId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PatchTransferHttpTrigger));

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("Unable to locate 'TouchpointId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestResult();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _logger.LogWarning("Unable to locate 'apimURL' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to locate 'CustomerId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult(customerGuid.ToString());
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _logger.LogWarning("Unable to locate 'InteractionId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult(interactionGuid.ToString());
            }

            if (!Guid.TryParse(transferId, out var transferGuid))
            {
                _logger.LogWarning("Unable to locate 'TransferId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult(transferGuid.ToString());
            }

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}. Correlation GUID: {CorrelationGuid}", touchpointId, correlationGuid);

            Models.TransferPatch transferPatchRequest;

            try
            {
                _logger.LogInformation("Attempting to retrieve resource from request body. Correlation GUID: {CorrelationGuid}", correlationGuid);
                transferPatchRequest = await _httpRequestHelper.GetResourceFromRequest<Models.TransferPatch>(req);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unable to parse {TransferPatch} from request body. Correlation GUID: {CorrelationGuid}. Exception: {ExceptionMessage}", nameof(transferPatchRequest), correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
            }

            if (transferPatchRequest == null)
            {
                _logger.LogInformation("{TransferPatch} object is NULL. Correlation GUID: {CorrelationGuid}", nameof(transferPatchRequest), correlationGuid);
                return new UnprocessableEntityResult();
            }

            _logger.LogInformation("Retrieved resource from request body. Correlation GUID: {CorrelationGuid}", correlationGuid);

            transferPatchRequest.LastModifiedTouchpointId = touchpointId;

            _logger.LogInformation("Attempting to validate {TransferPatch} object", nameof(transferPatchRequest));
            var errors = _validate.ValidateResource(transferPatchRequest, false);

            if (errors != null && errors.Any())
            {
                _logger.LogWarning("Falied to validate {TransferPatch} object", nameof(transferPatchRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Successfully validated {TransferPatch} object", nameof(transferPatchRequest));

            _logger.LogInformation("Attempting to check if customer exists. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer does not exist. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to check if customer is read only. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogWarning("Customer is read-only. Operation is forbidden. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to check Interaction exists for customer. Customer GUID: {CustomerGuid}. Interaction GUID: {InteractionGuid}", customerGuid, interactionGuid);
            var doesInteractionExist = await _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _logger.LogWarning("Interaction does not exist for customer. Customer GUID: {CustomerGuid}. Interaction GUID: {InteractionGuid}", customerGuid, interactionGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Interaction exists for customer. Customer GUID: {CustomerGuid}. Interaction GUID: {InteractionGuid}", customerGuid, interactionGuid);

            _logger.LogInformation("Attempting to retrieve Transfer for customer. Customer GUID: {CustomerGuid}. Transfer GUID: {TransferGuid}", customerGuid, transferGuid);
            var transfer = await _transferPatchService.GetTransferForCustomerAsync(customerGuid, transferGuid);

            if (transfer == null)
            {
                _logger.LogWarning("Transfer does not exist for customer. Customer GUID: {CustomerGuid}. Transfer GUID: {TransferGuid}", customerGuid, transferGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Transfer exists for customer. Customer GUID: {CustomerGuid}. Transfer GUID: {TransferGuid}", customerGuid, transferGuid);

            _logger.LogInformation("Attempting to PATCH Transfer. Customer GUID: {CustomerGuid}", customerGuid);
            var updatedTransfer = await _transferPatchService.UpdateAsync(transfer, transferPatchRequest);

            if (updatedTransfer != null)
            {
                _logger.LogInformation("Sending newly created Transfer to service bus. Customer GUID: {CustomerGuid}. Transfer ID: {TransferId}. Correlation GUID: {CorrelationGuid}", customerGuid, updatedTransfer.TransferId.GetValueOrDefault(), correlationGuid);
                await _transferPatchService.SendToServiceBusQueueAsync(transfer, customerGuid, ApimURL);
            }

            if (updatedTransfer == null)
            {
                _logger.LogWarning("PATCH request unsuccessful. Transfer GUID: {TransferGuid}", transferGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PatchTransferHttpTrigger));
                return new BadRequestObjectResult(transferGuid.ToString());
            }

            _logger.LogInformation("PATCH request successful. Transfer ID: {TransferId}", updatedTransfer.TransferId.GetValueOrDefault());
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PatchTransferHttpTrigger));
            return new JsonResult(updatedTransfer, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}