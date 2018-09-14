
using System;
using System.Configuration;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Transfer.Cosmos.Helper
{
    public class DocumentDBHelper : IDocumentDBHelper
    {
        private Uri _documentCollectionUri;
        private Uri _documentUri;
        private readonly string _databaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private readonly string _collectionId = ConfigurationManager.AppSettings["CollectionId"];

        private Uri _customerDocumentCollectionUri;
        private readonly string _customerDatabaseId = ConfigurationManager.AppSettings["CustomerDatabaseId"];
        private readonly string _customerCollectionId = ConfigurationManager.AppSettings["CustomerCollectionId"];

        private Uri _interactionDocumentCollectionUri;
        private readonly string _interactionDatabaseId = ConfigurationManager.AppSettings["InteractionDatabaseId"];
        private readonly string _interactionCollectionId = ConfigurationManager.AppSettings["InteractionCollectionId"];

        private Uri _subscriptionDocumentCollectionUri;
        private readonly string _subscriptionDatabaseId = ConfigurationManager.AppSettings["SubscriptionDatabaseId"];
        private readonly string _subscriptionCollectionId = ConfigurationManager.AppSettings["SubscriptionCollectionId"];

        public Uri CreateDocumentCollectionUri()
        {
            if (_documentCollectionUri != null)
                return _documentCollectionUri;

            _documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _databaseId,
                _collectionId);

            return _documentCollectionUri;
        }
        
        public Uri CreateDocumentUri(Guid transferId)
        {
            if (_documentUri != null)
                return _documentUri;

            _documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, transferId.ToString());

            return _documentUri;

        }

        #region CustomerDB

        public Uri CreateCustomerDocumentCollectionUri()
        {
            if (_customerDocumentCollectionUri != null)
                return _customerDocumentCollectionUri;

            _customerDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _customerDatabaseId, _customerCollectionId);

            return _customerDocumentCollectionUri;
        }

        #endregion

        #region InteractionDB

        public Uri CreateInteractionDocumentCollectionUri()
        {
            if (_interactionDocumentCollectionUri != null)
                return _interactionDocumentCollectionUri;

            _interactionDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _interactionDatabaseId, _interactionCollectionId);

            return _interactionDocumentCollectionUri;
        }

        #endregion

        #region SubscriptionDB

        public Uri CreateSubscriptionDocumentCollectionUri()
        {
            if (_subscriptionDocumentCollectionUri != null)
                return _subscriptionDocumentCollectionUri;

            _subscriptionDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _subscriptionDatabaseId, _subscriptionCollectionId);

            return _subscriptionDocumentCollectionUri;
        }

        #endregion   

    }
}
