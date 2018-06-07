using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Transfer.Models
{
    public class Transfer
    {
        public Guid TransferId { get; set; }

        [Required]
        public Guid InteractionId { get; set; }

        [Required]
        public Guid OriginatingTouchpointId { get; set; }

        [Required]
        public Guid TargetTouchpointId { get; set; }

        public string Context { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateandTimeOfTransfer { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateandTimeofTransferAccepted { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime RequestedCallbackTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ActualCallbackTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastModifiedDate { get; set; }

        public Guid LastModifiedTouchpointId { get; set; }
    }
}