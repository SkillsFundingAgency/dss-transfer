using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Transfer.Annotations;

namespace NCS.DSS.Transfer.Models
{
    public class Transfer
    {
        [Display(Description = "Unique identifier of the transfer record.")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        public Guid TransferId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of a customer.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid CustomerId { get; set; }

        [Required]
        [Display(Description = "Unique identifier for the related interaction record.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid InteractionId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of the touchpoint performing the transfer.")]
        [Example(Description = "a1e6d62b-b641-452d-bb25-12286f0a7ebc")]
        public Guid OriginatingTouchpointId { get; set; }

        [Required]
        [Example(Description = "91c56db7-f7a4-45af-aa4e-f0fd6c1a26cd")]
        public Guid TargetTouchpointId { get; set; }

        [StringLength(2000)]
        [Display(Description = "Context of the transfer.")]
        [Example(Description = "this is some text")]
        public string Context { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the transfer request was made.")]
        [Example(Description = "2018-06-20T13:45:00")]
        public DateTime DateandTimeOfTransfer { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the transfer request was accepted by the target touchpoint.")]
        [Example(Description = "2018-06-21T08:45:00")]
        public DateTime DateandTimeofTransferAccepted { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the customer wants to be called back.")]
        [Example(Description = "2018-06-27T08:45:00")]
        public DateTime RequestedCallbackTime { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the transfer was acted upon and the customer was contacted.")]
        [Example(Description = "2018-06-26T08:45:00")]
        public DateTime ActualCallbackTime { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-28T08:00:00")]
        public DateTime LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record.")]
        [Example(Description = "d1307d77-af23-4cb4-b600-a60e04f8c3df")]
        public Guid LastModifiedTouchpointId { get; set; }
    }
}