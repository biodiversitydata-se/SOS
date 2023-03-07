using SOS.DataStewardship.Api.Contracts.Enums;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    [SwaggerSchema("Associate Media", Required = new[] { "License" })]
    public class AssociatedMedia
    {        
        [SwaggerSchema("Name of attachment.")]
        public string AssociatedMediaName { get; set; }
        
        [SwaggerSchema("Format of attachment, e.g. image, video, sound, pdf etc.")]
        public AssociatedMediaType? AssociatedMediaType { get; set; }

        [SwaggerSchema("Link to attachment.")]
        public string AssociatedMediaLink { get; set; }

        [Required]
        [SwaggerSchema("States which Creative Commons license that is applied to the attachment.")]
        public string License { get; set; }

        [SwaggerSchema("States who the creator of the attachment is.")]
        public string RightsHolder { get; set; }
    }
}