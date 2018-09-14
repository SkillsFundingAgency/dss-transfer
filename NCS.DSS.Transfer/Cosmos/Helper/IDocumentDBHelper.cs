using System;

namespace NCS.DSS.Transfer.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        Uri CreateDocumentCollectionUri();
        Uri CreateDocumentUri(Guid transferId);
        Uri CreateCustomerDocumentCollectionUri();
        Uri CreateInteractionDocumentCollectionUri();
        Uri CreateSubscriptionDocumentCollectionUri();
    }
}