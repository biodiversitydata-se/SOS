using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Contracts.Enums
{
    /// <summary>
    /// Date filter type.
    /// </summary>
    public enum DateFilterType
    {
        /// <summary>
        /// Start OR EndDate of the event may be within the specified interval
        /// </summary>
        [EnumMember(Value = "OverlappingStartDateAndEndDate")]
        OverlappingStartDateAndEndDate = 0,
        /// <summary>
        /// Start AND EndDate of the event must be within the specified interval
        /// </summary>
        [EnumMember(Value = "BetweenStartDateAndEndDate")]
        BetweenStartDateAndEndDate = 1,
        /// <summary>
        /// Only StartDate of the event must be within the specified interval
        /// </summary>
        [EnumMember(Value = "OnlyStartDate")]
        OnlyStartDate = 2,
        /// <summary>
        /// Only EndDate of the event must be within the specified interval
        /// </summary>
        [EnumMember(Value = "OnlyEndDate")]
        OnlyEndDate = 3
    }
}