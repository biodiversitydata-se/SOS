using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Models.Processed.DataStewardship.Enums
{
    /// <summary>
    /// Format of attachment, e.g. image, video, sound, pdf etc.
    /// </summary>
    public enum AssociatedMediaType
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
}
