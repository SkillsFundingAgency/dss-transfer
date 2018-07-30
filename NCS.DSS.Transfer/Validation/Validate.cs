using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Transfer.Models;

namespace NCS.DSS.Transfer.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(ITransfer resource)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(resource, context, results, true);
            ValidateTransferRules(resource, results);

            return results;
        }

        private void ValidateTransferRules(ITransfer transferResource, List<ValidationResult> results)
        {
            if (transferResource == null)
                return;

            if (string.IsNullOrWhiteSpace(transferResource.Context))
                results.Add(new ValidationResult("Action Summary is a required field", new[] { "ActionSummary" }));

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
