using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.GetTransferHttpTrigger.Service
{
    public class GetTransferHttpTriggerService : IGetTransferHttpTriggerService
    {
        public async Task<List<Models.Transfer>> GetTransfers()
        {
            var result = CreateTempTransfers();
            return await Task.FromResult(result);
        }

        public List<Models.Transfer> CreateTempTransfers()
        {
            var transfersList = new List<Models.Transfer>
            {
                new Models.Transfer
                {
                    TransferId = Guid.Parse("caa2dd16-77dd-4041-955c-a53e3c48e0d1"),
                    InteractionId = Guid.NewGuid(),
                    OriginatingTouchpointId = Guid.NewGuid(),
                    TargetTouchpointId = Guid.NewGuid(),
                    DateandTimeOfTransfer = DateTime.Today,
                    DateandTimeofTransferAccepted = DateTime.Today.AddDays(10),
                    RequestedCallbackTime = DateTime.Today.AddDays(2),
                    ActualCallbackTime = DateTime.Today.AddDays(3),
                    LastModifiedDate = DateTime.Today.AddYears(1),
                    LastModifiedTouchpointId = Guid.NewGuid()
                },
                new Models.Transfer
                {
                    TransferId = Guid.Parse("45dde641-87bc-4c5b-9e26-aa663591a7c6"),
                    InteractionId = Guid.NewGuid(),
                    OriginatingTouchpointId = Guid.NewGuid(),
                    TargetTouchpointId = Guid.NewGuid(),
                    DateandTimeOfTransfer = DateTime.Today,
                    DateandTimeofTransferAccepted = DateTime.Today.AddDays(20),
                    RequestedCallbackTime = DateTime.Today.AddDays(4),
                    ActualCallbackTime = DateTime.Today.AddDays(6),
                    LastModifiedDate = DateTime.Today.AddYears(2),
                    LastModifiedTouchpointId = Guid.NewGuid()
                },
                new Models.Transfer
                {
                    TransferId = Guid.Parse("0ecbe9e5-368d-491b-b521-453ff953dc7d"),
                    InteractionId = Guid.NewGuid(),
                    OriginatingTouchpointId = Guid.NewGuid(),
                    TargetTouchpointId = Guid.NewGuid(),
                    DateandTimeOfTransfer = DateTime.Today,
                    DateandTimeofTransferAccepted = DateTime.Today.AddDays(30),
                    RequestedCallbackTime = DateTime.Today.AddDays(8),
                    ActualCallbackTime = DateTime.Today.AddDays(12),
                    LastModifiedDate = DateTime.Today.AddYears(3),
                    LastModifiedTouchpointId = Guid.NewGuid()
                }
            };

            return transfersList;
        }

    }
}
