using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Transfer.Cosmos.Client;
using NCS.DSS.Transfer.Cosmos.Helper;

namespace NCS.DSS.Transfer.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider()
        {
            _documentDbHelper = new DocumentDBHelper();
            _databaseClient = new DocumentDBClient();
        }

        public bool DoesCustomerResourceExist(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var customerQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return customerQuery.Where(x => x.Id == customerId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public bool DoesInteractionResourceExist(Guid interactionId)
        {
            var collectionUri = _documentDbHelper.CreateInteractionDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var interactionQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return interactionQuery.Where(x => x.Id == interactionId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public async Task<List<Models.Transfer>> GetTransfersForCustomerAsync(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var transfersQuery = client.CreateDocumentQuery<Models.Transfer>(collectionUri)
                .Where(so => so.CustomerId == customerId).AsDocumentQuery();

            var transfers = new List<Models.Transfer>();

            while (transfersQuery.HasMoreResults)
            {
                var response = await transfersQuery.ExecuteNextAsync<Models.Transfer>();
                transfers.AddRange(response);
            }

            return transfers.Any() ? transfers : null;
        }

        public async Task<Models.Transfer> GetTransferForCustomerAsync(Guid customerId, Guid transferId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var transferForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Transfer>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.TransferId == transferId)
                .AsDocumentQuery();

            if (transferForCustomerQuery == null)
                return null;

            var transfers = await transferForCustomerQuery.ExecuteNextAsync<Models.Transfer>();

            return transfers?.FirstOrDefault();
        }


        public async Task<ResourceResponse<Document>> CreateTransferAsync(Models.Transfer transfer)
        {

            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, transfer);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateTransferAsync(Models.Transfer transfer)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(transfer.TransferId.GetValueOrDefault());

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, transfer);

            return response;
        }
    }
}