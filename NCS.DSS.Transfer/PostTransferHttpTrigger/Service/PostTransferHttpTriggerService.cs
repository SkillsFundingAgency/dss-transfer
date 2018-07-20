using System;

namespace NCS.DSS.Transfer.PostTransferHttpTrigger.Service
{
    public class PostTransferHttpTriggerService : IPostTransferHttpTriggerService
    {
        public Guid? Create(Models.Transfer transfer)
        {
            if (transfer == null)
                return null;

            return Guid.NewGuid();
        }
    }
}