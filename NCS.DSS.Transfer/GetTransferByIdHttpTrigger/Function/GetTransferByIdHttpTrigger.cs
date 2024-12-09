using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Function
{
    public class GetTransferByIdHttpTrigger
    {
        private readonly IGetTransferByIdHttpTriggerService _transferByIdService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<GetTransferByIdHttpTrigger> _logger;

        public GetTransferByIdHttpTrigger(
            IGetTransferByIdHttpTriggerService transferByIdService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            ILogger<GetTransferByIdHttpTrigger> logger)
        {
            _transferByIdService = transferByIdService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _logger = logger;
        }

        [Function("GetById")]
        [ProducesResponseType(typeof(Models.Transfer), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Transfer found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Transfer does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "GET_BY_TRANSFERID", Description = "Ability to retrieve an individual transfer record.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")] HttpRequest req, string customerId, string interactionId, string transferId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(GetTransferByIdHttpTrigger));
            
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

            if (!Guid.TryParse(transferId, out var transferGuid))
            {
                _logger.LogWarning("Unable to locate 'TransferId' in request header");
                return new BadRequestObjectResult(transferGuid.ToString());
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

            _logger.LogInformation("Attempting to retrieve Transfer for customer. Customer GUID: {CustomerGuid}. Transfer GUID: {TransferGuid}", customerGuid, transferGuid);
            var transfer = await _transferByIdService.GetTransferForCustomerAsync(customerGuid, transferGuid);

            if (transfer == null)
            {
                _logger.LogInformation("Transfer does not exist for customer. Customer GUID: {CustomerGuid}. Transfer GUID: {TransferGuid}", customerGuid, transferGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetTransferByIdHttpTrigger));
                return new NoContentResult();
            }

            _logger.LogInformation("Transfer successfully retrieved. Customer GUID: {CustomerGuid}. Transfer GUID: {TransferGuid}", customerGuid, transferGuid);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetTransferByIdHttpTrigger));
            return new JsonResult(transfer, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}