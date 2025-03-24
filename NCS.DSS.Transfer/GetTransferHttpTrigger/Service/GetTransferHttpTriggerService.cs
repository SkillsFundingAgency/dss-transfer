using NCS.DSS.Transfer.Cosmos.Provider;

namespace NCS.DSS.Transfer.GetTransferHttpTrigger.Service
{
    public class GetTransferHttpTriggerService : IGetTransferHttpTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDBProvider;

        public GetTransferHttpTriggerService(ICosmosDBProvider cosmosDBProvider)
        {
            _cosmosDBProvider = cosmosDBProvider;
        }

        public async Task<List<Models.Transfer>> GetTransfersAsync(Guid customerId)
        {
            return await _cosmosDBProvider.GetTransfersForCustomerAsync(customerId);
        }
    }
}
