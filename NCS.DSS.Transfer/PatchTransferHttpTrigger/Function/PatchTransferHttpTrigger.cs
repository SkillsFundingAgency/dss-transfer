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
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ILogger _logger;
        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public PatchTransferHttpTrigger(
            IPatchTransferHttpTriggerService transferPatchService,
            IHttpRequestHelper httpRequestMessageHelper, 
            IResourceHelper resourceHelper, 
            IValidate validate, 
            IDynamicHelper dynamicHelper,
            ILogger<PatchTransferHttpTrigger> logger)
        {
            _transferPatchService = transferPatchService;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
            _logger = logger;
        }

        [Function("Patch")]
        [ProducesResponseType(typeof(Models.Transfer), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Transfer Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Transfer does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Transfer validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an transfer record.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")]HttpRequest req, string customerId, string interactionId, string transferId)
        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return new BadRequestResult();
            }

            var ApimURL = _httpRequestMessageHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _logger.LogInformation("Unable to locate 'apimurl' in request header");
                return new BadRequestResult();
            }

            _logger.LogInformation("Patch Transfer C# HTTP trigger function processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return new BadRequestObjectResult(customerGuid.ToString());

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return new BadRequestObjectResult(interactionGuid.ToString());

            if (!Guid.TryParse(transferId, out var transferGuid))
                return new BadRequestObjectResult(transferGuid.ToString());

            Models.TransferPatch transferPatchRequest;

            try
            {
                transferPatchRequest = await _httpRequestMessageHelper.GetResourceFromRequest<Models.TransferPatch>(req);
            }
            catch (JsonException ex)
            {
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
            }

            if (transferPatchRequest == null)
                return new UnprocessableEntityResult();

            transferPatchRequest.LastModifiedTouchpointId = touchpointId;

            var errors = _validate.ValidateResource(transferPatchRequest, false);

            if (errors != null && errors.Any())
                return new UnprocessableEntityObjectResult(errors);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return new NoContentResult();

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };

            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return new NoContentResult();

            var transfer = await _transferPatchService.GetTransferForCustomerAsync(customerGuid, transferGuid);

            if (transfer == null)
                return new NoContentResult();

            var updatedTransfer = await _transferPatchService.UpdateAsync(transfer, transferPatchRequest);

            if (updatedTransfer != null)
                await _transferPatchService.SendToServiceBusQueueAsync(transfer, customerGuid, ApimURL);

            return updatedTransfer == null
                ? new BadRequestObjectResult(transferGuid.ToString())
                : new JsonResult(updatedTransfer, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
        }
    }
}