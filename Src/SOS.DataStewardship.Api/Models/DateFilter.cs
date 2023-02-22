using System;
using System.Runtime.Serialization;
using SOS.DataStewardship.Api.Models.Enums;

namespace SOS.DataStewardship.Api.Models
{
    /// <summary>
    /// Date filter.
    /// </summary>
    [DataContract]
    public class DateFilter
    { 
        /// <summary>
        /// Event start date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed
        /// </summary>
        [DataMember(Name="startDate")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Event end date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed
        /// </summary>
        [DataMember(Name="endDate")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// DateFilterType
        /// </summary>
        [DataMember(Name= "dateFilterType")]
        public DateFilterType DateFilterType { get; set; }
    }
}
