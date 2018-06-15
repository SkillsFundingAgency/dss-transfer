using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Transfer.Annotations;
using Newtonsoft.Json;

namespace NCS.DSS.Transfer.DeleteTransferHttpTrigger
{
    public static class DeleteTransferHttpTrigger
    {
        [FunctionName("Delete")]
        [TransferResponse(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Transfer deleted", ShowSchema = true)]
        [TransferResponse(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Supplied Transfer Id does not exist", ShowSchema = false)]
        [Display(Name = "Delete", Description = "Ability to delete an transfer record.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string transferId)
        {
            log.Info("Delete Transfer C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Deleted Transfer record with Id of : " + transferGuid)
            };
        }
    }
}