using NCS.DSS.Transfer.Models;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Transfer.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(ITransfer resource, bool validateModelForPost)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(resource, context, results, true);
            ValidateTransferRules(resource, results, validateModelForPost);

            return results;
        }

        private void ValidateTransferRules(ITransfer transferResource, List<ValidationResult> results, bool validateModelForPost)
        {
            if (transferResource == null)
                return;

            if (validateModelForPost)
            {
                if (string.IsNullOrWhiteSpace(transferResource.Context))
                    results.Add(new ValidationResult("Context must have a value", new[] { "Context" }));
            }

            if (transferResource.DateandTimeOfTransfer.HasValue && transferResource.DateandTimeOfTransfer.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Date and Time Of Transfer must be less the current date/time", new[] { "DateandTimeOfTransfer" }));

            if (transferResource.DateandTimeofTransferAccepted.HasValue && transferResource.DateandTimeofTransferAccepted.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Date and Time of Transfer Accepted must be less the current date/time", new[] { "DateandTimeofTransferAccepted" }));

            if (transferResource.ActualCallbackTime.HasValue && transferResource.ActualCallbackTime.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Actual Callback Time must be less the current date/time", new[] { "ActualCallbackTime" }));

            if (transferResource.LastModifiedDate.HasValue && transferResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));

        }

    }
}
