using System;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.Cosmos.Helper
{
    public interface IResourceHelper
    {
        bool DoesCustomerExist(Guid customerId);
        Task<bool> IsCustomerReadOnly(Guid customerId);
        bool DoesInteractionExist(Guid interactionId);
    }
}