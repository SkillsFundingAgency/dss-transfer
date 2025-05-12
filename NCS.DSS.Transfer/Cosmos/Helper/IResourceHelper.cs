namespace NCS.DSS.Transfer.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        Task<bool> IsCustomerReadOnly(Guid customerId);
        Task<bool> DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId);
    }
}