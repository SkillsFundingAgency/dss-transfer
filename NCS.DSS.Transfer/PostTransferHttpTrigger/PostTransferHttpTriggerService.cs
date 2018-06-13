using System;

namespace NCS.DSS.Transfer.PostTransferHttpTrigger
{
    public class PostTransferHttpTriggerService
    {
        public Guid? Create(Models.Transfer transfer)
        {
            if (transfer == null)
                return null;

            return Guid.NewGuid();
        }
    }
}