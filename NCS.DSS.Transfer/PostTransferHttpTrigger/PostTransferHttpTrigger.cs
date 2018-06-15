using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Transfer.Annotations;

namespace NCS.DSS.Transfer.PostTransferHttpTrigger
{
    public static class PostTransferHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Transfer))]
        [TransferResponse(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Transfer Created", ShowSchema = true)]
        [TransferResponse(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to create Transfer", ShowSchema = false)]
        [TransferResponse(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Forbidden", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new transfer resource.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId)
        {
            log.Info("Post Transfer C# HTTP trigger function processed a request.");

            // Get request body
            var transfer = await req.Content.ReadAsAsync<Models.Transfer>();

            var transferService = new PostTransferHttpTriggerService();
            var transferId = transferService.Create(transfer);

            return transferId == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                : new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("Created Transfer record with Id of : " + transferId)
                };
        }
    }
}