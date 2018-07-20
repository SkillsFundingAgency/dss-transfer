using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Transfer.Annotations;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.Helpers;
using NCS.DSS.Transfer.Ioc;
using NCS.DSS.Transfer.PatchTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Transfer.PatchTransferHttpTrigger.Function
{
    public static class PatchTransferHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.Transfer))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Transfer Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Transfer does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Transfer validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an transfer record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string transferId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPatchTransferHttpTriggerService transferPatchService)
        {
            log.LogInformation("Patch Transfer C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(transferId, out var transferGuid))
                return HttpResponseMessageHelper.BadRequest(transferGuid);

            Models.TransferPatch transferPatchRequest;

            try
            {
                transferPatchRequest = await httpRequestMessageHelper.GetTransferFromRequest<Models.TransferPatch>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (transferPatchRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            var errors = validate.ValidateResource(transferPatchRequest);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = resourceHelper.DoesInteractionExist(interactionGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var transfer = await transferPatchService.GetTransferForCustomerAsync(customerGuid, transferGuid);

            if (transfer == null)
                return HttpResponseMessageHelper.NoContent(transferGuid);

            var updatedTransfer = await transferPatchService.UpdateAsync(transfer, transferPatchRequest);

            return updatedTransfer == null ?
                HttpResponseMessageHelper.BadRequest(transferGuid) :
                HttpResponseMessageHelper.Ok(updatedTransfer);
        }
    }
}