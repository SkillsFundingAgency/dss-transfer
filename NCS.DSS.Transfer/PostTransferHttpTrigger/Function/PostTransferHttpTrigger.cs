using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using NCS.DSS.Transfer.Annotations;
using NCS.DSS.Transfer.PostTransferHttpTrigger.Service;

namespace NCS.DSS.Transfer.PostTransferHttpTrigger.Function
{
    public static class PostTransferHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Transfer))]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Transfer Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Transfer does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Transfer validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new transfer resource.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/")]HttpRequestMessage req, ILogger log, string customerId, string interactionId)
        {
            log.LogInformation("Post Transfer C# HTTP trigger function processed a request.");

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