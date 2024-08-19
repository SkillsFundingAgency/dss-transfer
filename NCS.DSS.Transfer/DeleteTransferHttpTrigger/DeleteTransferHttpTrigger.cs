namespace NCS.DSS.Transfer.DeleteTransferHttpTrigger
{
    /*public static class DeleteTransferHttpTrigger
    {
        [Function("Delete")]
        [Display(Name = "Delete", Description = "Ability to delete an transfer record.")]
        public static IActionResult Run([Microsoft.Azure.Functions.Worker.HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string transferId)
        {
            log.LogInformation("Delete Transfer C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(transferId, out var transferGuid))
            {
                return new BadRequestObjectResult(transferId);
            }

            return new JsonResult("Deleted Transfer record with Id of : " + transferGuid, new JsonSerializerSettings())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }*/
}