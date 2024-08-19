using NCS.DSS.Transfer.Models;

namespace NCS.DSS.Transfer.Cosmos.Helper
{
    public interface ISubscriptionHelper
    {
        Task<Subscriptions> CreateSubscriptionAsync(Models.Transfer transfer);
        Task<List<Subscriptions>> GetSubscriptionsAsync(Guid customerGuid);
    }
}