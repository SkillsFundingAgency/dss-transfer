using System;

namespace NCS.DSS.Transfer.Cosmos.Helper
{
    public interface IResourceHelper
    {
        bool DoesCustomerExist(Guid customerId);
        bool DoesInteractionExist(Guid interactionId);
    }
}