using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models
{
    /// <summary>
    /// Associated media
    /// </summary>
    [DataContract]
    public class AssociatedMedia
    {
        /// <summary>
        /// Name of attachment.
        /// </summary>

        [DataMember(Name = "associatedMediaName")]
        public string AssociatedMediaName { get; set; }

        /// <summary>
        /// Format of attachment, e.g. image, video, sound, pdf etc.
        /// </summary>
        public enum AssociatedMediaTypeEnum
        {
            /// <summary>
            /// bild
            /// </summary>
            [EnumMember(Value = "bild")]
            Bild = 0,
            /// <summary>
            /// film
            /// </summary>
            [EnumMember(Value = "film")]
            Film = 1,
            /// <summary>
            /// ljud
            /// </summary>
            [EnumMember(Value = "ljud")]
            Ljud = 2,
            /// <summary>
            /// pdf
            /// </summary>
            [EnumMember(Value = "pdf")]
            Pdf = 3
        }

        /// <summary>
        /// Format of attachment, e.g. image, video, sound, pdf etc.
        /// </summary>
        [DataMember(Name = "associatedMediaType")]
        public AssociatedMediaTypeEnum? AssociatedMediaType { get; set; }

        /// <summary>
        /// Link to attachment.
        /// </summary>
        [DataMember(Name = "associatedMediaLink")]
        public string AssociatedMediaLink { get; set; }

        /// <summary>
        /// States which Creative Commons license that is applied to the attachment.
        /// </summary>
        [Required]
        [DataMember(Name = "license")]
        public string License { get; set; }

        /// <summary>
        /// States who the creator of the attachment is.
        /// </summary>
        [DataMember(Name = "rightsHolder")]
        public string RightsHolder { get; set; }
    }
}