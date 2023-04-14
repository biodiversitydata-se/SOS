using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace SOS.Observations.Api.Dtos.DataStewardship
{
    [SwaggerSchema("The methodology that is used for the data collection, e.g. as described by one or more monitoring manuals. Can contain a range of methods for the same and/or different parts of the data collection.")]
    public class MethodologyDto
    {
        [Required]
        [SwaggerSchema("The title or name of a methodology, e.g. a monitoring manual.")]
        public string MethodologyName { get; set; }

        [Required]
        [SwaggerSchema("Short description of a methodology, e.g. a monitoring manual.")]
        public string MethodologyDescription { get; set; }

        [Required]
        [SwaggerSchema("Persistent link to description of a methodology, e.g. a monitoring manual.")]
        public string MethodologyLink { get; set; }

        [Required]
        [SwaggerSchema("Persistent link to published species list for the dataset.")]
        public string SpeciesList { get; set; }
    }
}
