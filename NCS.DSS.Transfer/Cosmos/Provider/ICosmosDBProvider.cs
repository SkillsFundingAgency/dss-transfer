using Microsoft.Azure.Cosmos;
using NCS.DSS.Transfer.Models;

namespace NCS.DSS.Transfer.Cosmos.Provider
{
    public interface ICosmosDBProvider
    {
        Task<bool> DoesCustomerResourceExistAsync(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDateAsync(Guid customerId);
        Task<bool> DoesInteractionResourceExistAndBelongToCustomerAsync(Guid interactionId, Guid customerId);
        Task<List<Models.Transfer>> GetTransfersForCustomerAsync(Guid customerId);
        Task<Models.Transfer> GetTransferForCustomerAsync(Guid customerId, Guid transferId);
        Task<ItemResponse<Models.Transfer>> CreateTransferAsync(Models.Transfer transfer);
        Task<ItemResponse<Models.Transfer>> UpdateTransferAsync(Models.Transfer transfer);
        Task<List<Models.Subscription>> GetSubscriptionsByCustomerIdAsync(Guid? customerId);
        Task<Subscription> CreateSubscriptionAsync(Models.Transfer transfer);
    }
}