using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Transfer.Annotations;
using Newtonsoft.Json;

namespace NCS.DSS.Transfer.PatchTransferHttpTrigger
{
    public static class PatchTransferHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.Transfer))]
        [TransferResponse(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Transfer updated", ShowSchema = true)]
        [TransferResponse(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Supplied Transfer Id does not exist", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an transfer record.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string transferId)
        {
            log.Info("Patch Transfer C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Updated Transfer record with Id of : " + transferGuid)
            };
        }
    }
}