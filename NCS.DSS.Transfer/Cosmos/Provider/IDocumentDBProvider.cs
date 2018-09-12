using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Transfer.Models;

namespace NCS.DSS.Transfer.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        bool DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        bool DoesInteractionResourceExist(Guid interactionId);
        Task<List<Models.Transfer>> GetTransfersForCustomerAsync(Guid customerId);
        Task<Models.Transfer> GetTransferForCustomerAsync(Guid customerId, Guid transferId);
        Task<ResourceResponse<Document>> CreateTransferAsync(Models.Transfer transfer);
        Task<ResourceResponse<Document>> UpdateTransferAsync(Models.Transfer transfer);
        Task<List<Models.Subscriptions>> GetSubscriptionsByCustomerIdAsync(Guid? customerId);
        Task<ResourceResponse<Document>> CreateSubscriptionsAsync(Models.Subscriptions subscriptions);
    }
}