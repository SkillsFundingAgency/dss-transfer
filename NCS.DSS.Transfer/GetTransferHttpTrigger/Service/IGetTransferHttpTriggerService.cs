using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.GetTransferHttpTrigger.Service
{
    public interface IGetTransferHttpTriggerService
    {
        Task<List<Models.Transfer>> GetTransfersAsync(Guid customerId);
    }
}