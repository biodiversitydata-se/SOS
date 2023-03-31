using System.Runtime.Serialization;

namespace SOS.Lib.Enums.Weather
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
        PartlyClear3To5Av8 = 0,
        /// <summary>
        /// klart, 0/8
        /// </summary>
        [EnumMember(Value = "klart, 0/8")]
        Clear0Of8 = 1,
        /// <summary>
        /// molnigt, 6-7/8
        /// </summary>
        [EnumMember(Value = "molnigt, 6-7/8")]
        Cloudy6To7Of8 = 2,
        /// <summary>
        /// mulet, 8/8
        /// </summary>
        [EnumMember(Value = "mulet, 8/8")]
        Overcast8Of8 = 3,
        /// <summary>
        /// nästan klart, 1-2/8
        /// </summary>
        [EnumMember(Value = "nästan klart, 1-2/8")]
        AlmostClear1To2Av8 = 4,
        /// <summary>
        /// växlande, 0-8/8
        /// </summary>
        [EnumMember(Value = "växlande, 0-8/8")]
        EverChanging0Till8Av8 = 5
    }
}
