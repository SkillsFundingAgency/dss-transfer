using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.GetTransferByIdHttpTrigger
{
    public static class GetTransferByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [Display(Name = "Get", Description = "Ability to retrieve an individual transfer record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string transferId)
        {
            log.Info("Get Transfer By Id C# HTTP trigger function  processed a request.");

            if (!Guid.TryParse(transferId, out var transferGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(transferId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var service = new GetTransferByIdHttpTriggerService();
            var values = await service.GetTransfer(transferGuid);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}