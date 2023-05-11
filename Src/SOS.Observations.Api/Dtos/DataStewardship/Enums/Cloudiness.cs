using System.Runtime.Serialization;

namespace SOS.Observations.Api.Dtos.DataStewardship.Enums
{
    /// <summary>
    /// States the cloud condtions during the survey event.
    /// </summary>
    public enum Cloudiness
    {
        /// <summary>
        /// halvklart, 3-5/8
        /// </summary>
        [EnumMember(Value = "halvklart, 3-5/8")]
        Halvklart3Till5Av8 = 0,
        /// <summary>
        /// klart, 0/8
        /// </summary>
        [EnumMember(Value = "klart, 0/8")]
        Klart0Av8 = 1,
        /// <summary>
        /// molnigt, 6-7/8
        /// </summary>
        [EnumMember(Value = "molnigt, 6-7/8")]
        Molnigt6Till7Av8 = 2,
        /// <summary>
        /// mulet, 8/8
        /// </summary>
        [EnumMember(Value = "mulet, 8/8")]
        Mulet8Av8 = 3,
        /// <summary>
        /// nästan klart, 1-2/8
        /// </summary>
        [EnumMember(Value = "nästan klart, 1-2/8")]
        NästanKlart1Till2Av8 = 4,
        /// <summary>
        /// växlande, 0-8/8
        /// </summary>
        [EnumMember(Value = "växlande, 0-8/8")]
        Växlande0Till8Av8 = 5
    }
}