using System.Runtime.Serialization;

namespace SOS.Lib.Models.Processed.DataStewardship.Enums
{
    /// <summary>
    /// The program area within environmental monitoring that the dataset belongs to.
    /// </summary>    
    public enum ProgrammeArea
    {
        /// <summary>
        /// Biogeografisk uppföljning av naturtyper och arter
        /// </summary>
        [EnumMember(Value = "Biogeografisk uppföljning av naturtyper och arter")]
        BiogeografiskUppföljningAvNaturtyperOchArter = 0,
        /// <summary>
        /// Fjäll
        /// </summary>
        [EnumMember(Value = "Fjäll")]
        Fjäll = 1,
        /// <summary>
        /// Jordbruksmark
        /// </summary>
        [EnumMember(Value = "Jordbruksmark")]
        Jordbruksmark = 2,
        /// <summary>
        /// Kust och hav
        /// </summary>
        [EnumMember(Value = "Kust och hav")]
        KustOchHav = 3,
        /// <summary>
        /// Landskap
        /// </summary>
        [EnumMember(Value = "Landskap")]
        Landskap = 4,
        /// <summary>
        /// Skog
        /// </summary>
        [EnumMember(Value = "Skog")]
        Skog = 5,
        /// <summary>
        /// Skog
        /// </summary>
        [EnumMember(Value = "Våtmark")]
        Våtmark = 6
    }
}