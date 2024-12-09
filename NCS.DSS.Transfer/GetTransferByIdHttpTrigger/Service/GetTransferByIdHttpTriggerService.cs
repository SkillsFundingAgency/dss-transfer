using NCS.DSS.Transfer.Cosmos.Provider;

namespace NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Service
{
    public class GetTransferByIdHttpTriggerService : IGetTransferByIdHttpTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDBProvider;

        public GetTransferByIdHttpTriggerService(ICosmosDBProvider cosmosDBProvider)
        {
            _cosmosDBProvider = cosmosDBProvider;
        }

        public async Task<Models.Transfer> GetTransferForCustomerAsync(Guid customerId, Guid transferId)
        {
            var transfer = await _cosmosDBProvider.GetTransferForCustomerAsync(customerId, transferId);

            return transfer;
        }
    }
}