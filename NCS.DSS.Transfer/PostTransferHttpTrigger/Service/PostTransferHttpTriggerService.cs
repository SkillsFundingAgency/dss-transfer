using NCS.DSS.Transfer.Cosmos.Provider;
using NCS.DSS.Transfer.ServiceBus;
using System.Net;

namespace NCS.DSS.Transfer.PostTransferHttpTrigger.Service
{
    public class PostTransferHttpTriggerService : IPostTransferHttpTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDBProvider;
        private readonly IServiceBusClient _serviceBusClient;

        public PostTransferHttpTriggerService(ICosmosDBProvider cosmosDBProvider, IServiceBusClient serviceBusClient)
        {
            _cosmosDBProvider = cosmosDBProvider;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Models.Transfer> CreateAsync(Models.Transfer transfer)
        {
            if (transfer == null)
            {
                return null;
            }

            transfer.SetDefaultValues();

            var response = await _cosmosDBProvider.CreateTransferAsync(transfer);

            return response.StatusCode == HttpStatusCode.Created ? response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Transfer transfer, string reqUrl)
        {
            await _serviceBusClient.SendPostMessageAsync(transfer, reqUrl);
        }
    }
}