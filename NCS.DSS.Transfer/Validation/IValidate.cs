using NCS.DSS.Transfer.Models;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Transfer.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(ITransfer resource, bool validateModelForPost);
    }
}