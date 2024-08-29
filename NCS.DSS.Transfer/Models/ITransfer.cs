namespace NCS.DSS.Transfer.Models
{
    public interface ITransfer
    {
        string TargetTouchpointId { get; set; }
        string Context { get; set; }
        DateTime? DateandTimeOfTransfer { get; set; }
        DateTime? DateandTimeofTransferAccepted { get; set; }
        DateTime? RequestedCallbackTime { get; set; }
        DateTime? ActualCallbackTime { get; set; }
        DateTime? LastModifiedDate { get; set; }
        string LastModifiedTouchpointId { get; set; }

        void SetDefaultValues();

    }
}