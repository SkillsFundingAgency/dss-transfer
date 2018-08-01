using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Transfer.Cosmos.Provider;

namespace NCS.DSS.Transfer.GetTransferHttpTrigger.Service
{
    public class GetTransferHttpTriggerService : IGetTransferHttpTriggerService
    {
        public async Task<List<Models.Transfer>> GetTransfersAsync(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var transfers = await documentDbProvider.GetTransfersForCustomerAsync(customerId);

            return transfers;
        }
    }
}
