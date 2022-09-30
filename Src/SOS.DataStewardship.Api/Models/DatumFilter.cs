using System;
using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// Date filter.
    /// </summary>
    [DataContract]
    public class DatumFilter
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
        /// DatumFilterType
        /// </summary>
        [DataMember(Name="datumFilterType")]
        public DatumFilterType DatumFilterType { get; set; }
    }
}
