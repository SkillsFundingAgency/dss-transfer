using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Transfer.Models
{
    public class Transfer
    {
        [Display(Description = "Unique identifier of the transfer record.")]
        public Guid TransferId { get; set; }

        [Required]
        [Display(Description = "Unique identifier for the related interaction record.")]
        public Guid InteractionId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of the touchpoint performing the transfer.")]
        public Guid OriginatingTouchpointId { get; set; }

        [Required]
        public Guid TargetTouchpointId { get; set; }

        [StringLength(2000)]
        [Display(Description = "Context of the transfer.")]
        public string Context { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the transfer request was made.")]
        public DateTime DateandTimeOfTransfer { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the transfer request was accepted by the target touchpoint.")]
        public DateTime DateandTimeofTransferAccepted { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the customer wants to be called back.")]
        public DateTime RequestedCallbackTime { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the transfer was acted upon and the customer was contacted.")]
        public DateTime ActualCallbackTime { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        public DateTime LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record.")]
        public Guid LastModifiedTouchpointId { get; set; }
    }
}