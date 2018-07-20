using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Transfer.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}