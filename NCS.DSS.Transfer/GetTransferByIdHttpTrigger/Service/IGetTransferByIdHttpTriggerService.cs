using System;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Service
{
    public interface IGetTransferByIdHttpTriggerService
    {
        Task<Models.Transfer> GetTransferForCustomerAsync(Guid customerId, Guid transferId);
    }
}