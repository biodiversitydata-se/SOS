using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Contracts.Enums
{
    /// <summary>
    /// "The type of survey site that was surveyed, e.g. square, segment, point site, counting zone, route etc."
    /// </summary>
    public enum LocationType
    {
        /// <summary>
        ///  Block for block
        /// </summary>
        [EnumMember(Value = "block")]
        Block = 0,
        /// <summary>
        ///  Linje for linje
        /// </summary>
        [EnumMember(Value = "linje")]
        Linje = 1,
        /// <summary>
        ///  Linjetransekt for linjetransekt
        /// </summary>
        [EnumMember(Value = "linjetransekt")]
        Linjetransekt = 2,
        /// <summary>
        ///  Polygon for polygon
        /// </summary>
        [EnumMember(Value = "polygon")]
        Polygon = 3,
        /// <summary>
        ///  Provyta for provyta
        /// </summary>
        [EnumMember(Value = "provyta")]
        Provyta = 4,
        /// <summary>
        ///  Punkt for punkt
        /// </summary>
        [EnumMember(Value = "punkt")]
        Punkt = 5,
        /// <summary>
        ///  Punktlokal for punktlokal
        /// </summary>
        [EnumMember(Value = "punktlokal")]
        Punktlokal = 6,
        /// <summary>
        ///  Ruta for ruta
        /// </summary>
        [EnumMember(Value = "ruta")]
        Ruta = 7,
        /// <summary>
        ///  Rutt for rutt
        /// </summary>
        [EnumMember(Value = "rutt")]
        Rutt = 8,
        /// <summary>
        ///  Rkningssektor for räkningssektor
        /// </summary>
        [EnumMember(Value = "räkningssektor")]
        Räkningssektor = 9,
        /// <summary>
        ///  Rkningszon for räkningszon
        /// </summary>
        [EnumMember(Value = "räkningszon")]
        Räkningszon = 10,
        /// <summary>
        ///  Segment for segment
        /// </summary>
        [EnumMember(Value = "segment")]
        Segment = 11,
        /// <summary>
        ///  Slinga for slinga
        /// </summary>
        [EnumMember(Value = "slinga")]
        Slinga = 12,
        /// <summary>
        ///  Transportstrcka for transportsträcka
        /// </summary>
        [EnumMember(Value = "transportsträcka")]
        Transportsträcka = 13,
        /// <summary>
        ///   for ö
        /// </summary>
        [EnumMember(Value = "ö")]
        Ö = 14
    }
}