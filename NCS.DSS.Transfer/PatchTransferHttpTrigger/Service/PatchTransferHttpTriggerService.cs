using System.Net;
using NCS.DSS.Transfer.Cosmos.Provider;
using NCS.DSS.Transfer.Models;
using NCS.DSS.Transfer.ServiceBus;

namespace NCS.DSS.Transfer.PatchTransferHttpTrigger.Service
{
    public class PatchTransferHttpTriggerService : IPatchTransferHttpTriggerService
    {
        public async Task<Models.Transfer> UpdateAsync(Models.Transfer transfer, TransferPatch transferPatch)
        {
            if (transferPatch == null)
                return null;

            transferPatch.SetDefaultValues();
            transfer.Patch(transferPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateTransferAsync(transfer);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? transfer : null;
        }

        public async Task<Models.Transfer> GetTransferForCustomerAsync(Guid customerId, Guid transferId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var transfer = await documentDbProvider.GetTransferForCustomerAsync(customerId, transferId);

            return transfer;
        }

        public async Task SendToServiceBusQueueAsync(Models.Transfer transfer, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(transfer, customerId, reqUrl);
        }
    }
}