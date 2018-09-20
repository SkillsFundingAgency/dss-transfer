using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Extensions.Logging;
using NCS.DSS.Transfer.Annotations;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.GetTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Helpers;
using NCS.DSS.Transfer.Ioc;

namespace NCS.DSS.Transfer.GetTransferHttpTrigger.Function
{
    public static class GetTransferHttpTrigger
    {
        [FunctionName("Get")]
        [ResponseType(typeof(Models.Transfer))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Transfers found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Transfers do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return all transfer records for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/")]HttpRequestMessage req, ILogger log, string customerId, string interactionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IGetTransferHttpTriggerService transferGetService)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Get Transfers C# HTTP trigger function processed a request. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = await resourceHelper.DoesInteractionExist(interactionGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var transfers = await transferGetService.GetTransfersAsync(customerGuid);

            return transfers == null ?
                HttpResponseMessageHelper.NoContent(interactionGuid) :
                HttpResponseMessageHelper.Ok(JsonHelper.SerializeObjects(transfers));

        }
    }
}