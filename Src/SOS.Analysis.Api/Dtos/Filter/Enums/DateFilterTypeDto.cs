namespace SOS.Analysis.Api.Dtos.Filter.Enums
{
    /// <summary>
    /// OverlappingStartDateAndEndDate, Start or EndDate of the observation must be within the specified interval    
    /// BetweenStartDateAndEndDate, Start and EndDate of the observation must be within the specified interval    
    /// OnlyStartDate, Only StartDate of the observation must be within the specified interval            
    /// OnlyEndDate, Only EndDate of the observation must be within the specified interval    
    /// </summary>
    public enum DateFilterTypeDto
    {
        /// <summary>
        /// Start or EndDate of the observation must be within the specified interval
        /// </summary>
        OverlappingStartDateAndEndDate,
        /// <summary>
        /// Start and EndDate of the observation must be within the specified interval
        /// </summary>
        BetweenStartDateAndEndDate,
        /// <summary>
        /// Only StartDate of the observation must be within the specified interval
        /// </summary>
        OnlyStartDate,
        /// <summary>
        /// Only EndDate of the observation must be within the specified interval
        /// </summary>
        OnlyEndDate
    }
}
