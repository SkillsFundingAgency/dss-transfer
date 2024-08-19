using System.ComponentModel.DataAnnotations;
using NCS.DSS.Transfer.Models;

namespace NCS.DSS.Transfer.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(ITransfer resource, bool validateModelForPost);
    }
}