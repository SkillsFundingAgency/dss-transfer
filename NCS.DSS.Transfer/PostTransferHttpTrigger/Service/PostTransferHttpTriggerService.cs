using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Transfer.Cosmos.Provider;
using NCS.DSS.Transfer.ServiceBus;

namespace NCS.DSS.Transfer.PostTransferHttpTrigger.Service
{
    public class PostTransferHttpTriggerService : IPostTransferHttpTriggerService
    {
        public async Task<Models.Transfer> CreateAsync(Models.Transfer transfer)
        {
            if (transfer == null)
                return null;

            transfer.SetDefaultValues();

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateTransferAsync(transfer);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Transfer transfer, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync_Target(transfer, reqUrl);
            await ServiceBusClient.SendPostMessageAsync_Originator(transfer, reqUrl);
        }
    }
}