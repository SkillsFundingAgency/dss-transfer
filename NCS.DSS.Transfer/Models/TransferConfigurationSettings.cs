namespace NCS.DSS.Transfer.Models
{
    public class TransferConfigurationSettings
    {
        public required string BaseAddress { get; set; }
        public required string KeyName { get; set; }
        public required string AccessKey { get; set; }
        public required string QueueName { get; set; }
        public required string CosmosDbEndpoint { get; set; }
        public required string Key { get; set; }
        public required string TransferConnectionString { get; set; } = string.Empty;
        public required string ServiceBusConnectionString { get; set; } = string.Empty;
        public required string DatabaseId { get; set; }
        public required string CollectionId { get; set; }
        public required string CustomerDatabaseId { get; set; }
        public required string CustomerCollectionId { get; set; }
        public required string InteractionDatabaseId { get; set; }
        public required string InteractionCollectionId { get; set; }
        public required string SubscriptionDatabaseId { get; set; }
        public required string SubscriptionCollectionId { get; set; }
    }
}
