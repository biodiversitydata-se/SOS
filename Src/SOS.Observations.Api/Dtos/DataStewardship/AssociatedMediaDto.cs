using Newtonsoft.Json;
using SOS.Lib.Models.Processed.DataStewardship.Enums;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace SOS.Observations.Api.Dtos.DataStewardship
{
    /// <summary>
    /// Associate Media
    /// </summary>
    [SwaggerSchema("Associate Media", Required = new[] { "License" })]
    public class AssociatedMediaDto
    {
        /// <summary>
        /// Name of attachment
        /// </summary>
        [SwaggerSchema("Name of attachment.")]
        public string AssociatedMediaName { get; set; }

        /// <summary>
        /// Format of attachment, e.g. image, video, sound, pdf etc.
        /// </summary>
        [SwaggerSchema("Format of attachment, e.g. image, video, sound, pdf etc.")]
        public AssociatedMediaType? AssociatedMediaType { get; set; }

        /// <summary>
        /// Link to attachment
        /// </summary>
        [SwaggerSchema("Link to attachment.")]
        public string AssociatedMediaLink { get; set; }

        /// <summary>
        /// States which Creative Commons license that is applied to the attachment
        /// </summary>
        [Required]
        [SwaggerSchema("States which Creative Commons license that is applied to the attachment.")]
        public string License { get; set; }

        /// <summary>
        /// States who the creator of the attachment is
        /// </summary>
        [SwaggerSchema("States who the creator of the attachment is.")]
        public string RightsHolder { get; set; }
    }
}
