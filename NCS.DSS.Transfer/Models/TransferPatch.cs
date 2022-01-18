using System;
using System.ComponentModel.DataAnnotations;
using DFC.Swagger.Standard.Annotations;

namespace NCS.DSS.Transfer.Models
{
    public class TransferPatch : ITransfer
    {
        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string TargetTouchpointId { get; set; }

        [StringLength(2000)]
        [Display(Description = "Context of the transfer.")]
        [Example(Description = "this is some text")]
        public string Context { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the transfer request was made.")]
        [Example(Description = "2018-06-20T13:45:00")]
        public DateTime? DateandTimeOfTransfer { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the transfer request was accepted by the target touchpoint.")]
        [Example(Description = "2018-06-21T08:45:00")]
        public DateTime? DateandTimeofTransferAccepted { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the customer wants to be called back.")]
        [Example(Description = "2018-06-27T08:45:00")]
        public DateTime? RequestedCallbackTime { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the transfer was acted upon and the customer was contacted.")]
        [Example(Description = "2018-06-26T08:45:00")]
        public DateTime? ActualCallbackTime { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-28T08:00:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointId { get; set; }

        public void SetDefaultValues()
        {
            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;
        }
    }
}