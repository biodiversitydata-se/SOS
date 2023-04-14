using System.Runtime.Serialization;

namespace SOS.Observations.Api.Dtos.DataStewardship.Enums
{
    /// <summary>
    /// States how the quantity of the observed species (or other taxonomic rank) was counted or measured.
    /// </summary>
    public enum QuantityVariable
    {
        /// <summary>
        ///  Antal individer
        /// </summary>
        [EnumMember(Value = "antal individer")]
        AntalIndivider = 0,
        /// <summary>
        ///  Antal fruktkroppar
        /// </summary>
        [EnumMember(Value = "antal fruktkroppar")]
        AntalFruktkroppar = 1,
        /// <summary>
        ///  Antal kapslar
        /// </summary>
        [EnumMember(Value = "antal kapslar")]
        AntalKapslar = 2,
        /// <summary>
        ///  Antal plantor/tuvor
        /// </summary>
        [EnumMember(Value = "antal plantor/tuvor")]
        AntalPlantorTuvor = 3,
        /// <summary>
        ///  Antal stjälkar/strån/skott
        /// </summary>
        [EnumMember(Value = "antal stjälkar/strån/skott")]
        AntalStjälkarStrånSkott = 4,
        /// <summary>
        ///  Antal äggklumpar
        /// </summary>
        [EnumMember(Value = "antal äggklumpar")]
        AntalÄggklumpar = 5,
        /// <summary>
        ///  Täckningsgrad
        /// </summary>
        [EnumMember(Value = "täckningsgrad")]
        Täckningsgrad = 6,
        /// <summary>
        ///  Yttäckning
        /// </summary>
        [EnumMember(Value = "yttäckning")]
        Yttäckning = 7
    }

}

