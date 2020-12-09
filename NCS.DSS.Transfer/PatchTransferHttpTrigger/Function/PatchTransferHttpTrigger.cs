using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Transfer.Annotations;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.Helpers;
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
        private readonly IHttpRequestMessageHelper _httpRequestMessageHelper;
        private readonly IValidate _validate;
        private readonly IPatchTransferHttpTriggerService _transferPatchService;

        public PatchTransferHttpTrigger(IResourceHelper resourceHelper, IHttpRequestMessageHelper httpRequestMessageHelper, IValidate validate, IPatchTransferHttpTriggerService transferPatchService)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _validate = validate;
            _transferPatchService = transferPatchService;
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
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string transferId)
        {
            var touchpointId = _httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            var ApimURL = _httpRequestMessageHelper.GetApimURL(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Patch Transfer C# HTTP trigger function processed a request. By Touchpoint. " + touchpointId); 

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(transferId, out var transferGuid))
                return HttpResponseMessageHelper.BadRequest(transferGuid);

            Models.TransferPatch transferPatchRequest;

            try
            {
                transferPatchRequest = await _httpRequestMessageHelper.GetTransferFromRequest<Models.TransferPatch>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (transferPatchRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            transferPatchRequest.LastModifiedTouchpointId = touchpointId;

            var errors = _validate.ValidateResource(transferPatchRequest, false);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return HttpResponseMessageHelper.Forbidden(customerGuid);

            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var transfer = await _transferPatchService.GetTransferForCustomerAsync(customerGuid, transferGuid);

            if (transfer == null)
                return HttpResponseMessageHelper.NoContent(transferGuid);

            var updatedTransfer = await _transferPatchService.UpdateAsync(transfer, transferPatchRequest);

            if (updatedTransfer != null)
                await _transferPatchService.SendToServiceBusQueueAsync(transfer, customerGuid, ApimURL);

            return updatedTransfer == null ?
                HttpResponseMessageHelper.BadRequest(transferGuid) :
                HttpResponseMessageHelper.Ok(JsonHelper.SerializeObject(updatedTransfer));
        }
    }
}