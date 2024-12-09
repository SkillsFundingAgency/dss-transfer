using NCS.DSS.Transfer.Cosmos.Provider;

namespace NCS.DSS.Transfer.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        private readonly ICosmosDBProvider _cosmosDBProvider;

        public ResourceHelper(ICosmosDBProvider cosmosDBProvider)
        {
            _cosmosDBProvider = cosmosDBProvider;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _cosmosDBProvider.DoesCustomerResourceExistAsync(customerId);
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            return await _cosmosDBProvider.DoesCustomerHaveATerminationDateAsync(customerId);
        }

        public async Task<bool> DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            return await _cosmosDBProvider.DoesInteractionResourceExistAndBelongToCustomerAsync(interactionId, customerId);
        }
    }
}
