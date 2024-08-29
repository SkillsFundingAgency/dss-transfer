namespace NCS.DSS.Transfer.GetTransferHttpTrigger.Service
{
    public interface IGetTransferHttpTriggerService
    {
        Task<List<Models.Transfer>> GetTransfersAsync(Guid customerId);
    }
}