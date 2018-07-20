using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Transfer.Cosmos.Provider;

namespace NCS.DSS.Transfer.PostTransferHttpTrigger.Service
{
    public class PostTransferHttpTriggerService : IPostTransferHttpTriggerService
    {
        public async Task<Models.Transfer> CreateAsync(Models.Transfer transfer)
        {
            if (transfer == null)
                return null;

            var transferId = Guid.NewGuid();
            transfer.TransferId = transferId;

            if (!transfer.LastModifiedDate.HasValue)
                transfer.LastModifiedDate = DateTime.Now;

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateTransferAsync(transfer);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }
    }
}