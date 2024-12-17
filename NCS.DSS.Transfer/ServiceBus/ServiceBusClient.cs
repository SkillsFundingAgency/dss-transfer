using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Transfer.Cosmos.Provider;
using NCS.DSS.Transfer.Models;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.Transfer.ServiceBus
{

    public class ServiceBusClient : IServiceBusClient
    {
        private readonly ServiceBusSender _serviceBusSender;
        private readonly ICosmosDBProvider _cosmosDBProvider;
        private readonly ILogger<ServiceBusClient> _logger;

        public ServiceBusClient(Azure.Messaging.ServiceBus.ServiceBusClient serviceBusClient, ICosmosDBProvider cosmosDBProvider, IOptions<TransferConfigurationSettings> configOptions, ILogger<ServiceBusClient> logger)
        {
            var config = configOptions.Value;

            _serviceBusSender = serviceBusClient.CreateSender(config.QueueName);
            _cosmosDBProvider = cosmosDBProvider;
            _logger = logger;
        }

        public async Task CheckAndCreateSubscription(Models.Transfer transfer)
        {
            var subscriptions = await _cosmosDBProvider.GetSubscriptionsByCustomerIdAsync(transfer.CustomerId);
            var doesSubscriptionExist = subscriptions != null && subscriptions.Any(x =>
                                            x.CustomerId == transfer.CustomerId &&
                                            x.TouchPointId == transfer.TargetTouchpointId);

            if (doesSubscriptionExist == false)
            {
                await _cosmosDBProvider.CreateSubscriptionAsync(transfer);
            }
        }

        public async Task SendPostMessageAsync(Models.Transfer transfer, string reqUrl)
        {
            _logger.LogInformation(
                "Starting {MethodName}. Transfer ID: {TransferId}. Customer ID: {CustomerId}",
                nameof(SendPostMessageAsync), transfer.TransferId, transfer.CustomerId);

            try
            {
                var messageModel = new MessageModel()
                {
                    TitleMessage = $"New Transfer record {transfer.CustomerId} added at {DateTime.UtcNow}",
                    CustomerGuid = transfer.CustomerId,
                    LastModifiedDate = transfer.LastModifiedDate,
                    URL = $"{reqUrl}/{transfer.TransferId}",
                    IsNewCustomer = false,
                    TouchpointId = transfer.LastModifiedTouchpointId
                };

                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
                {
                    ContentType = "application/json",
                    MessageId = $"{transfer.CustomerId} {DateTime.UtcNow}"
                };

                await CheckAndCreateSubscription(transfer);
                await _serviceBusSender.SendMessageAsync(msg);

                _logger.LogInformation(
                    "Successfully completed {MethodName}. Transfer ID: {TransferId}. Customer ID: {CustomerId}",
                    nameof(SendPostMessageAsync), transfer.TransferId, transfer.CustomerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred in {MethodName}. Transfer ID: {TransferId}. Customer ID: {CustomerId}",
                    nameof(SendPostMessageAsync), transfer.TransferId, transfer.CustomerId);
                throw;
            }
        }

        public async Task SendPatchMessageAsync(Models.Transfer transfer, Guid customerId, string reqUrl)
        {
            _logger.LogInformation(
                "Starting {MethodName}. Transfer ID: {TransferId}. Customer ID: {CustomerId}",
                nameof(SendPatchMessageAsync), transfer.TransferId, customerId);

            try
            {
                var messageModel = new MessageModel
                {
                    TitleMessage = $"Transfer record modification for {customerId} at {DateTime.UtcNow}",
                    CustomerGuid = customerId,
                    LastModifiedDate = transfer.LastModifiedDate,
                    URL = reqUrl,
                    IsNewCustomer = false,
                    TouchpointId = transfer.LastModifiedTouchpointId
                };

                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
                {
                    ContentType = "application/json",
                    MessageId = $"{customerId} {DateTime.UtcNow}"
                };

                await CheckAndCreateSubscription(transfer);
                await _serviceBusSender.SendMessageAsync(msg);

                _logger.LogInformation(
                    "Successfully completed {MethodName}. Transfer ID: {TransferId}. Customer ID: {CustomerId}",
                    nameof(SendPatchMessageAsync), transfer.TransferId, customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred in {MethodName}. Transfer ID: {TransferId}. Customer ID: {CustomerId}",
                    nameof(SendPatchMessageAsync), transfer.TransferId, customerId);
                throw;
            }
        }
    }
}

