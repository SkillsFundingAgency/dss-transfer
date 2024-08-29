namespace NCS.DSS.Transfer.PutTransferHttpTrigger.Function
{
    /*    public static class PutTransferHttpTrigger
        {
            [Function("PUT")]
            public static IActionResult Run([Microsoft.Azure.Functions.Worker.HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")] HttpRequest req, ILogger log, string customerId, string interactionId, string transferId)
            {
                log.LogInformation("Put Transfer C# HTTP trigger function processed a request.");

                if (!Guid.TryParse(transferId, out var transferGuid))
                {
                    return new BadRequestObjectResult(transferId);
                }

                return new JsonResult("Replaced Transfer record with Id of : " + transferGuid, new JsonSerializerSettings())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
        }*/
}