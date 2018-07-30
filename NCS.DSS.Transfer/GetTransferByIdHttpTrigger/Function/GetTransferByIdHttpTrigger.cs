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
using NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Service;
using NCS.DSS.Transfer.Helpers;
using NCS.DSS.Transfer.Ioc;

namespace NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Function
{
    public static class GetTransferByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [ResponseType(typeof(Models.Transfer))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Transfer found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Transfer does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual transfer record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string transferId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetTransferByIdHttpTriggerService transferByIdService)
        {
            log.LogInformation("Get Transfer By Id C# HTTP trigger function  processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(transferId, out var transferGuid))
                return HttpResponseMessageHelper.BadRequest(transferGuid);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = resourceHelper.DoesInteractionExist(interactionGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var transfer = await transferByIdService.GetTransferForCustomerAsync(customerGuid, transferGuid);

            return transfer == null ?
                HttpResponseMessageHelper.NoContent(transferGuid) :
                HttpResponseMessageHelper.Ok(transfer);

        }
    }
}