using SOS.Lib.Models.Processed.DataStewardship.Enums;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SOS.Lib.Models.Processed.DataStewardship.Event
{
    /// <summary>
    /// Associated media
    /// </summary>
    public class AssociatedMedia
    {
        /// <summary>
        /// Name of attachment.
        /// </summary>
        public string AssociatedMediaName { get; set; }

        /// <summary>
        /// Format of attachment, e.g. image, video, sound, pdf etc.
        /// </summary>
        public AssociatedMediaType? AssociatedMediaType { get; set; }

        /// <summary>
        /// Link to attachment.
        /// </summary>
        public string AssociatedMediaLink { get; set; }

        /// <summary>
        /// States which Creative Commons license that is applied to the attachment.
        /// </summary>
        [Required]
        public string License { get; set; }

        /// <summary>
        /// States who the creator of the attachment is.
        /// </summary>
        public string RightsHolder { get; set; }
    }
}