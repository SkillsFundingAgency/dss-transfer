using NCS.DSS.Transfer.Cosmos.Provider;

namespace NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Service
{
    public class GetTransferByIdHttpTriggerService : IGetTransferByIdHttpTriggerService
    {
        public async Task<Models.Transfer> GetTransferForCustomerAsync(Guid customerId, Guid transferId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var transfer = await documentDbProvider.GetTransferForCustomerAsync(customerId, transferId);

            return transfer;
        }
    }
}