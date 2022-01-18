using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.PatchTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.PatchTransferHttpTrigger.Function
{
    public class PatchTransferHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IValidate _validate;
        private readonly IPatchTransferHttpTriggerService _transferPatchService;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IJsonHelper _jsonHelper;

        public PatchTransferHttpTrigger(IResourceHelper resourceHelper, IHttpRequestHelper httpRequestMessageHelper, IValidate validate, IPatchTransferHttpTriggerService transferPatchService, IHttpResponseMessageHelper httpResponseMessageHelper, IJsonHelper jsonHelper)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _validate = validate;
            _transferPatchService = transferPatchService;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
        }

        [FunctionName("Patch")]
        [ProducesResponseType(typeof(Models.Transfer), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Transfer Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Transfer does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Transfer validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an transfer record.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string transferId)
        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return _httpResponseMessageHelper.BadRequest();
            }

            var ApimURL = _httpRequestMessageHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Patch Transfer C# HTTP trigger function processed a request. By Touchpoint. " + touchpointId); 

            if (!Guid.TryParse(customerId, out var customerGuid))
                return _httpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return _httpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(transferId, out var transferGuid))
                return _httpResponseMessageHelper.BadRequest(transferGuid);

            Models.TransferPatch transferPatchRequest;

            try
            {
                transferPatchRequest = await _httpRequestMessageHelper.GetResourceFromRequest<Models.TransferPatch>(req);
            }
            catch (JsonException ex)
            {
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (transferPatchRequest == null)
                return _httpResponseMessageHelper.UnprocessableEntity();

            transferPatchRequest.LastModifiedTouchpointId = touchpointId;

            var errors = _validate.ValidateResource(transferPatchRequest, false);

            if (errors != null && errors.Any())
                return _httpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return _httpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return _httpResponseMessageHelper.Forbidden(customerGuid);

            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return _httpResponseMessageHelper.NoContent(interactionGuid);

            var transfer = await _transferPatchService.GetTransferForCustomerAsync(customerGuid, transferGuid);

            if (transfer == null)
                return _httpResponseMessageHelper.NoContent(transferGuid);

            var updatedTransfer = await _transferPatchService.UpdateAsync(transfer, transferPatchRequest);

            if (updatedTransfer != null)
                await _transferPatchService.SendToServiceBusQueueAsync(transfer, customerGuid, ApimURL);

            return updatedTransfer == null ?
                _httpResponseMessageHelper.BadRequest(transferGuid) :
                _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(updatedTransfer, "id", "TransferId"));
        }
    }
}