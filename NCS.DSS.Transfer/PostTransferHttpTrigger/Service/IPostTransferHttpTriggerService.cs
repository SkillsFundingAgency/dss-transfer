using System;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.PostTransferHttpTrigger.Service
{
    public interface IPostTransferHttpTriggerService
    {
        Task<Models.Transfer> CreateAsync(Models.Transfer transfer);
        Task SendToServiceBusQueueAsync(Models.Transfer transfer, string reqUrl);
    }
}