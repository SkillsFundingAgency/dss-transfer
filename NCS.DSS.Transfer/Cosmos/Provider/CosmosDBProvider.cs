using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.LearningProgression.Models;
using NCS.DSS.Transfer.Models;

namespace NCS.DSS.Transfer.Cosmos.Provider
{
    public class CosmosDBProvider : ICosmosDBProvider
    {
        private readonly Container _transferContainer;
        private readonly Container _customerContainer;
        private readonly Container _interactionContainer;
        private readonly Container _subscriptionContainer;
        private readonly PartitionKey _partitionKey = PartitionKey.None;
        private readonly ILogger<CosmosDBProvider> _logger;

        public CosmosDBProvider(CosmosClient cosmosClient, IOptions<TransferConfigurationSettings> configOptions, ILogger<CosmosDBProvider> logger)
        {
            var config = configOptions.Value;

            _transferContainer = GetContainer(cosmosClient, config.DatabaseId, config.CollectionId);
            _customerContainer = GetContainer(cosmosClient, config.CustomerDatabaseId, config.CustomerCollectionId);
            _interactionContainer = GetContainer(cosmosClient, config.InteractionDatabaseId, config.InteractionCollectionId);
            _subscriptionContainer = GetContainer(cosmosClient, config.SubscriptionDatabaseId, config.SubscriptionCollectionId);
            _logger = logger;
        }

        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
            => cosmosClient.GetContainer(databaseId, collectionId);

        public async Task<bool> DoesCustomerResourceExistAsync(Guid customerId)
        {
            try
            {
                var response = await _customerContainer.ReadItemAsync<Customer>(
                    customerId.ToString(),
                    _partitionKey);

                return response.Resource != null;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // If a 404 occurs, the resource does not exist
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking customer resource existence. Customer ID: {CustomerId}. Error message: {ErrorMessage}", customerId, ex.Message);
                throw;
            }
        }

        public async Task<bool> DoesInteractionResourceExistAndBelongToCustomerAsync(Guid interactionId, Guid customerId)
        {
            _logger.LogInformation("Checking for Interaction. Customer ID: {CustomerId} Interaction ID: {InteractionId}", customerId, interactionId);

            try
            {
                var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM interactions i WHERE i.id = @interactionId AND i.CustomerId = @customerId")
                    .WithParameter("@interactionId", interactionId)
                    .WithParameter("@customerId", customerId);

                var iterator = _interactionContainer.GetItemQueryIterator<long>(query);
                var response = await iterator.ReadNextAsync();
                var interactionFound = response.FirstOrDefault() > 0;

                _logger.LogInformation("Interaction check completed. CustomerId: {CustomerId}. InteractionFound: {interactionFound}", customerId, interactionFound);
                return interactionFound;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "An error occurred when checking for Interaction. CustomerId: {CustomerId}. InteractionId: {InteractionId}. Error message: {ErrorMessage}",customerId, interactionId, ce.Message);
                return false;
            }
        }

        public async Task<bool> DoesCustomerHaveATerminationDateAsync(Guid customerId)
        {
            _logger.LogInformation("Checking for termination date. Customer ID: {CustomerId}", customerId);

            try
            {
                var response = await _customerContainer.ReadItemAsync<Customer>(customerId.ToString(), PartitionKey.None);
                var dateOfTermination = response.Resource?.DateOfTermination;
                var hasTerminationDate = dateOfTermination != null;

                _logger.LogInformation("Termination date check completed. CustomerId: {CustomerId}. HasTerminationDate: {HasTerminationDate}", customerId, hasTerminationDate);
                return dateOfTermination != null;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // If a 404 occurs, the resource does not exist
                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured when checking termination date. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<List<Subscription>> GetSubscriptionsByCustomerIdAsync(Guid? customerId)
        {
            _logger.LogInformation("Retrieving subscriptions. CustomerId: {CustomerId}", customerId);

            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.CustomerId = @customerId AND c.Subscribe = true")
                    .WithParameter("@customerId", customerId);

                var subscriptions = new List<Subscription>();
                var iterator = _subscriptionContainer.GetItemQueryIterator<Subscription>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    subscriptions.AddRange(response.Resource);
                }

                return subscriptions.Any() ? subscriptions : null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "An error occured when retrieving Subscriptions. CustomerId: {CustomerId}", customerId);
                return null;
            }
        }

        public async Task<Subscription> CreateSubscriptionAsync(Models.Transfer transfer)
        {
            if (transfer == null)
            {
                return null;
            }

            try
            {
                _logger.LogInformation("Creating Subscription. Customer ID: {CustomerId}", transfer.CustomerId);

                var subscription = new Subscription
                {
                    SubscriptionId = Guid.NewGuid(),
                    CustomerId = transfer.CustomerId,
                    TouchPointId = transfer.TargetTouchpointId,
                    Subscribe = true,
                    LastModifiedDate = transfer.LastModifiedDate,

                };

                if (!transfer.LastModifiedDate.HasValue)
                {
                    subscription.LastModifiedDate = DateTime.Now;
                }

                var response = await _subscriptionContainer.CreateItemAsync(subscription, _partitionKey);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    _logger.LogInformation("Successfully created Subscription. CustomerId: {CustomerId}", transfer.CustomerId);
                    return response.Resource;
                }

                _logger.LogWarning("Failed to create Subscription. Customer ID: {CustomerId}. Response Code {StatusCode}", transfer.CustomerId, response.StatusCode);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce,"An error occurred when creating Subscription. Customer ID: {CustomerId}. Exception {ExceptionMessage}", transfer.CustomerId, ce.Message);
                throw;
            }
        }

        public async Task<List<Models.Transfer>> GetTransfersForCustomerAsync(Guid customerId)
        {
            _logger.LogInformation("Retrieving Transfers. CustomerId: {CustomerId}", customerId);

            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.CustomerId = @customerId")
                    .WithParameter("@customerId", customerId);

                var transfers = new List<Models.Transfer>();
                var iterator = _transferContainer.GetItemQueryIterator<Models.Transfer>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    transfers.AddRange(response.Resource);
                }

                _logger.LogInformation("Retrieved {Count} Transfers. CustomerId: {CustomerId}", transfers.Count, customerId);

                return transfers.Any() ? transfers : null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce,"An error occurred when retrieving Transfers. Customer ID: {CustomerId}. Exception {ExceptionMessage}", customerId, ce.Message);
                return null;
            }
        }

        public async Task<Models.Transfer> GetTransferForCustomerAsync(Guid customerId, Guid transferId)
        {
            _logger.LogInformation("Retrieving Transfer. CustomerId: {CustomerId}, TransferId: {TransferId}", customerId, transferId);

            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.CustomerId = @customerId AND c.TransferId = @transferId")
                    .WithParameter("@customerId", customerId)
                    .WithParameter("@transferId", transferId);

                var iterator = _transferContainer.GetItemQueryIterator<Models.Transfer>(query);
                var response = await iterator.ReadNextAsync();

                return response.Resource.FirstOrDefault();
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce,"An error occurred when retrieving Transfers. Customer ID: {CustomerId}. Exception {ExceptionMessage}", customerId, ce.Message);

                return null;
            }
        }
        public async Task<ItemResponse<Models.Transfer>> CreateTransferAsync(Models.Transfer transfer)
        {
            _logger.LogInformation("Creating Transfer. CustomerId: {CustomerId}", transfer.CustomerId);

            try
            {
                _logger.LogInformation("Successfully created Transfer. CustomerId: {CustomerId}", transfer.CustomerId);
                return await _transferContainer.CreateItemAsync(transfer, _partitionKey);

            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce,"An error occurred when creating Transfer. Customer ID: {CustomerId}. Exception {ExceptionMessage}", transfer.CustomerId, ce.Message);
                return null;
            }
        }

        public async Task<ItemResponse<Models.Transfer>> UpdateTransferAsync(Models.Transfer transfer)
        {
            _logger.LogInformation("Updating transfer. TransferId: {TransferId}. CustomerId: {CustomerId}", transfer.TransferId, transfer.CustomerId);

            try
            {
                var response = await _transferContainer.ReplaceItemAsync(transfer, transfer.TransferId.ToString(), _partitionKey);
                _logger.LogInformation("Successfully updated Transfer. TransferId: {TransferId}", transfer.TransferId);

                return response;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "An error occured when updating Transfer. Customer ID: {CustomerId}. Exception {ExceptionMessage}", transfer.CustomerId, ce.Message);
                return null;
            }
        }
    }
}