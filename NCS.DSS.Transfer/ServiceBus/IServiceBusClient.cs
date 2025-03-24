namespace NCS.DSS.Transfer.ServiceBus
{
    public interface IServiceBusClient
    {
        Task CheckAndCreateSubscription(Models.Transfer transfer);
        Task SendPostMessageAsync(Models.Transfer transfer, string reqUrl);
        Task SendPatchMessageAsync(Models.Transfer transfer, Guid customerId, string reqUrl);
    }
}
