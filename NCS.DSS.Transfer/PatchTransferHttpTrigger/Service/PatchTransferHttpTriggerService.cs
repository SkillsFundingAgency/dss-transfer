using NCS.DSS.Transfer.Cosmos.Provider;
using NCS.DSS.Transfer.Models;
using NCS.DSS.Transfer.ServiceBus;
using System.Net;

namespace NCS.DSS.Transfer.PatchTransferHttpTrigger.Service
{
    public class PatchTransferHttpTriggerService : IPatchTransferHttpTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDBProvider;
        private readonly IServiceBusClient _serviceBusClient;

        public PatchTransferHttpTriggerService(ICosmosDBProvider cosmosDBProvider, IServiceBusClient serviceBusClient)
        {
            _cosmosDBProvider = cosmosDBProvider;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Models.Transfer> UpdateAsync(Models.Transfer transfer, TransferPatch transferPatch)
        {
            if (transferPatch == null)
            {
                return null;
            }

            transferPatch.SetDefaultValues();
            transfer.Patch(transferPatch);

            var response = await _cosmosDBProvider.UpdateTransferAsync(transfer);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? transfer : null;
        }

        public async Task<Models.Transfer> GetTransferForCustomerAsync(Guid customerId, Guid transferId)
        {
            return await _cosmosDBProvider.GetTransferForCustomerAsync(customerId, transferId);
        }

        public async Task SendToServiceBusQueueAsync(Models.Transfer transfer, Guid customerId, string reqUrl)
        {
            await _serviceBusClient.SendPatchMessageAsync(transfer, customerId, reqUrl);
        }
    }
}