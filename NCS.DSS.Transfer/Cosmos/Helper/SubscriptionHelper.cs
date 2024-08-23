using NCS.DSS.Transfer.Cosmos.Provider;
using NCS.DSS.Transfer.Models;
using System.Net;

namespace NCS.DSS.Transfer.Cosmos.Helper
{
    public class SubscriptionHelper : ISubscriptionHelper
    {
        public async Task<Subscriptions> CreateSubscriptionAsync(Models.Transfer transfer)
        {
            if (transfer == null)
                return null;

            var subscription = new Subscriptions
            {
                SubscriptionId = Guid.NewGuid(),
                CustomerId = transfer.CustomerId,
                TouchPointId = transfer.TargetTouchpointId,
                Subscribe = true,
                LastModifiedDate = transfer.LastModifiedDate,

            };

            if (!transfer.LastModifiedDate.HasValue)
                subscription.LastModifiedDate = DateTime.Now;

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateSubscriptionsAsync(subscription);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : (Guid?)null;
        }

        public async Task<List<Subscriptions>> GetSubscriptionsAsync(Guid customerGuid)
        {
            var documentDbProvider = new DocumentDBProvider();
            var subscriptions = await documentDbProvider.GetSubscriptionsByCustomerIdAsync(customerGuid);

            return subscriptions;
        }

    }
}
