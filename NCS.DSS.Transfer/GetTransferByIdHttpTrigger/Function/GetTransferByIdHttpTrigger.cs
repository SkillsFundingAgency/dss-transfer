using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Transfer.Annotations;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Service;
using NCS.DSS.Transfer.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Function
{
    public class GetTransferByIdHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestMessageHelper _httpRequestMessageHelper;
        private readonly IGetTransferByIdHttpTriggerService _transferByIdService;
        public GetTransferByIdHttpTrigger(IResourceHelper resourceHelper, IHttpRequestMessageHelper httpRequestMessageHelper, IGetTransferByIdHttpTriggerService transferByIdService)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper =  httpRequestMessageHelper;
            _transferByIdService = transferByIdService;
        }

        [FunctionName("GetById")]
        [ProducesResponseType(typeof(Models.Transfer), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Transfer found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Transfer does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual transfer record.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/Transfers/{transferId}")] HttpRequestMessage req, ILogger log, string customerId, string interactionId, string transferId)
        {
            var touchpointId = _httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Get Transfer By Id C# HTTP trigger function  processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(transferId, out var transferGuid))
                return HttpResponseMessageHelper.BadRequest(transferGuid);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var transfer = await _transferByIdService.GetTransferForCustomerAsync(customerGuid, transferGuid);

            return transfer == null ?
                HttpResponseMessageHelper.NoContent(transferGuid) :
                HttpResponseMessageHelper.Ok(JsonHelper.SerializeObject(transfer));

        }
    }
}