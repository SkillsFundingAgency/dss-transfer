using Microsoft.Azure.ServiceBus;
using NCS.DSS.Transfer.Cosmos.Helper;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.ServiceBus
{

    public static class ServiceBusClient
    {
        public static readonly string KeyName = Environment.GetEnvironmentVariable("KeyName");
        public static readonly string AccessKey = Environment.GetEnvironmentVariable("AccessKey");
        public static readonly string BaseAddress = Environment.GetEnvironmentVariable("BaseAddress");
        public static readonly string QueueName = Environment.GetEnvironmentVariable("QueueName");
        private static readonly SubscriptionHelper _subscriptionHelper = new SubscriptionHelper();

        public static async Task CheckAndCreateSubscription(Models.Transfer transfer)
        {
            var subscriptions = await _subscriptionHelper.GetSubscriptionsAsync(transfer.CustomerId);
            var doesSubscriptionExist = subscriptions != null && subscriptions.Any(x =>
                                            x.CustomerId == transfer.CustomerId &&
                                            x.TouchPointId == transfer.TargetTouchpointId);
            
            if (doesSubscriptionExist == false)
            {
                await _subscriptionHelper.CreateSubscriptionAsync(transfer);
            }
        }
        
        public static async Task SendPostMessageAsync_Target(Models.Transfer transfer, string reqUrl)
        {
            var connectionString = GetConnectionString();
            var queueClient = new QueueClient(connectionString, QueueName);
            var messageModel = new MessageModel()
            {
                TitleMessage = "New Transfer record {" + transfer.TransferId + "} added at " + DateTime.UtcNow,
                CustomerGuid = transfer.CustomerId,
                LastModifiedDate = transfer.LastModifiedDate,
                URL = reqUrl + "/" + transfer.TransferId,
                IsNewCustomer = false,
                TouchpointId = transfer.LastModifiedTouchpointId
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = transfer.CustomerId + " " + DateTime.UtcNow
            };

            await CheckAndCreateSubscription(transfer);
            await queueClient.SendAsync(msg);
        }

        private static string GetConnectionString()
        {
            ServiceBusConnectionStringBuilder sb = new ServiceBusConnectionStringBuilder();
            sb.Endpoint = BaseAddress;
            sb.SasKey = AccessKey;
            sb.EntityPath = "tranfers";
            sb.SasKeyName = KeyName;
            var connectionString = sb.GetNamespaceConnectionString();
            return connectionString;
        }

        public static async Task SendPatchMessageAsync(Models.Transfer transfer, Guid customerId, string reqUrl)
        {
            var connectionString = GetConnectionString();

            var queueClient = new QueueClient(connectionString, QueueName);
            var messageModel = new MessageModel
            {
                TitleMessage = "Transfer record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = transfer.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = transfer.LastModifiedTouchpointId
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            await CheckAndCreateSubscription(transfer);
            await queueClient.SendAsync(msg);
        }

    }

    public class MessageModel
    {
        public string TitleMessage { get; set; }
        public Guid? CustomerGuid { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string URL { get; set; }
        public bool IsNewCustomer { get; set; }
        public string TouchpointId { get; set; }
    }

}

