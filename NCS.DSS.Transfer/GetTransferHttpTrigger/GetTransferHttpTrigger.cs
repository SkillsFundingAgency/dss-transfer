using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.GetTransferHttpTrigger
{
    public static class GetTransferHttpTrigger
    {
        [FunctionName("Get")]
        [Display(Name = "Get", Description = "Ability to return all transfer records for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId)
        {
            log.Info("Get Transfers C# HTTP trigger function processed a request.");

            var service = new GetTransferHttpTriggerService();
            var values = await service.GetTransfers();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}