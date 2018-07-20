using System;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.PatchTransferHttpTrigger.Service
{
    public interface IPatchTransferHttpTriggerService
    {
        Task<Models.Transfer> UpdateAsync(Models.Transfer transfer, Models.TransferPatch transferPatch);
        Task<Models.Transfer> GetTransferForCustomerAsync(Guid customerId, Guid transferId);
    }
}