using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models.Enums
{
    /// <summary>
    /// Type of project that the dataset was collected within, e.g. 'delprogram', 'gemensamt delprogram'.
    /// </summary>        
    public enum ProjectType
    {
        /// <summary>
        /// Artportalen projekt
        /// </summary>
        [EnumMember(Value = "artportalenprojekt")]
        Artportalenprojekt = 0,
        /// <summary>
        /// Delprogram
        /// </summary>
        [EnumMember(Value = "delprogram")]
        Delprogram = 1,
        /// <summary>
        /// Delsystem
        /// </summary>
        [EnumMember(Value = "delsystem")]
        Delsystem = 2,
        /// <summary>
        /// Delprogram
        /// </summary>
        [EnumMember(Value = "gemensamt delprogram")]
        GemensamtDelprogram = 3
    }
}