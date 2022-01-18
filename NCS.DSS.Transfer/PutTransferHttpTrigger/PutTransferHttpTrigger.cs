using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.Transfer.PutTransferHttpTrigger
{
    public static class PutTransferHttpTrigger
    {
        [Disable]
        [FunctionName("PUT")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string transferId)
        {
            log.Info("Put Transfer C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(transferId, out var transferGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(transferId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Replaced Transfer record with Id of : " + transferGuid)
            };
        }
    }
}