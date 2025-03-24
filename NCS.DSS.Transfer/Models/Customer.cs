using DFC.Swagger.Standard.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NCS.DSS.LearningProgression.Models
{
    public class Customer
    {
        [Display(Description = "Unique identifier of a customer")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [Display(Description = "Date the customer terminated their account")]
        [Example(Description = "2018-06-21T14:45:00")]
        public DateTime? DateOfTermination { get; set; }
    }    
}
