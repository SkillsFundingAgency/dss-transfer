using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace NCS.DSS.Transfer.ServiceBus
{
    public static class ServiceBusClient
    {
        public static readonly string KeyName = ConfigurationManager.AppSettings["KeyName"];
        public static readonly string AccessKey = ConfigurationManager.AppSettings["AccessKey"];
        public static readonly string BaseAddress = ConfigurationManager.AppSettings["BaseAddress"];
        public static readonly string QueueName = ConfigurationManager.AppSettings["QueueName"];

        public static async Task SendPostMessageAsync_Target(Models.Transfer transfer, string reqUrl)
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(KeyName, AccessKey);
            var messagingFactory = MessagingFactory.Create(BaseAddress, tokenProvider);
            var sender = messagingFactory.CreateMessageSender(QueueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Transfer record {" + transfer.TransferId + "} added at " + DateTime.UtcNow,
                CustomerGuid = transfer.CustomerId,
                LastModifiedDate = transfer.LastModifiedDate,
                URL = reqUrl + "/" + transfer.TransferId,
                IsNewCustomer = false,
                TouchpointId = transfer.LastModifiedTouchpointId
            };

            var msg = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel))))
            {
                ContentType = "application/json",
                MessageId = transfer.CustomerId + " " + DateTime.UtcNow
            };

            //msg.ForcePersistence = true; Required when we save message to cosmos
            await sender.SendAsync(msg);
        }

        public static async Task SendPostMessageAsync_Originator(Models.Transfer transfer, string reqUrl)
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(KeyName, AccessKey);
            var messagingFactory = MessagingFactory.Create(BaseAddress, tokenProvider);
            var sender = messagingFactory.CreateMessageSender(QueueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Transfer record {" + transfer.TransferId + "} added at " + DateTime.UtcNow,
                CustomerGuid = transfer.CustomerId,
                LastModifiedDate = transfer.LastModifiedDate,
                URL = reqUrl + "/" + transfer.TransferId,
                IsNewCustomer = false,
                TouchpointId = transfer.OriginatingTouchpointId
            };

            var msg = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel))))
            {
                ContentType = "application/json",
                MessageId = transfer.CustomerId + " " + DateTime.UtcNow
            };

            //msg.ForcePersistence = true; Required when we save message to cosmos
            await sender.SendAsync(msg);
        }


        public static async Task SendPatchMessageAsync(Models.Transfer transfer, Guid customerId, string reqUrl)
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(KeyName, AccessKey);
            var messagingFactory = MessagingFactory.Create(BaseAddress, tokenProvider);
            var sender = messagingFactory.CreateMessageSender(QueueName);
            var messageModel = new MessageModel
            {
                TitleMessage = "Transfer record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = transfer.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = transfer.LastModifiedTouchpointId
            };

            var msg = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel))))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            //msg.ForcePersistence = true; Required when we save message to cosmos
            await sender.SendAsync(msg);
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

