using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.GetTransferHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Transfer.GetTransferHttpTrigger.Function
{
    public class GetTransferHttpTrigger
    {
        private readonly IGetTransferHttpTriggerService _transferGetService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<GetTransferHttpTrigger> _logger;

        public GetTransferHttpTrigger(
            IGetTransferHttpTriggerService transferGetService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            ILogger<GetTransferHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestHelper = httpRequestHelper;
            _transferGetService = transferGetService;
            _logger = logger;
        }

        [Function("GET")]
        [ProducesResponseType(typeof(Models.Transfer), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Transfers found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Transfers do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "GET", Description = "Ability to return all transfer records for a given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/")] HttpRequest req, string customerId, string interactionId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(GetTransferHttpTrigger));

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("Unable to locate 'TouchpointId' in request header");
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to locate 'CustomerId' in request header");
                return new BadRequestObjectResult(customerGuid.ToString());
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _logger.LogWarning("Unable to locate 'InteractionId' in request header");
                return new BadRequestObjectResult(interactionGuid.ToString());
            }

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}", touchpointId);

            _logger.LogInformation("Attempting to see if customer exists. Customer GUID: {CustomerGuid}", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer does not exist. Customer GUID: {CustomerGuid}", customerGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}", customerGuid);

            _logger.LogInformation("Attempting to check Interaction exists for customer. Customer GUID: {CustomerGuid}", customerGuid);
            var doesInteractionExist = await _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _logger.LogWarning("Interaction does not exist for customer. Customer GUID: {CustomerGuid}. Interaction GUID: {InteractionGuid}", customerGuid, interactionGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Interaction exists for customer. Customer GUID: {CustomerGuid}. Interaction GUID: {InteractionGuid}", customerGuid, interactionGuid);
            
            _logger.LogInformation("Attempting to retrieve Transfer(s) for customer. Customer GUID: {CustomerGuid}", customerGuid);
            var transfers = await _transferGetService.GetTransfersAsync(customerGuid);

            if (transfers == null)
            {
                _logger.LogInformation("Transfer does not exist for customer. Customer GUID: {CustomerGuid}", customerGuid);
                return new NoContentResult();
            }

            if (transfers.Count == 1)
            {
                _logger.LogInformation("Transfer successfully retrieved. Customer GUID: {CustomerGuid}. Transfer GUID: {TransferGuid}", customerGuid, transfers[0].TransferId);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetTransferHttpTrigger));
                return new JsonResult(transfers[0], new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            var transferIds = transfers.Select(t => t.TransferId).ToList();

            _logger.LogInformation("Transfers successfully retrieved. Customer GUID: {CustomerGuid}. Transfer GUIDs: {TransferGuids}", customerGuid, transferIds);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetTransferHttpTrigger));
            return new JsonResult(transfers, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}