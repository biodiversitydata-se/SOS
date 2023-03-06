using System.Runtime.Serialization;
using SOS.DataStewardship.Api.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

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
        [SwaggerSchema("Event start date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Event end date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed
        /// </summary>
        [DataMember(Name="endDate")]
        [SwaggerSchema("Event end date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// DateFilterType
        /// </summary>
        [DataMember(Name= "dateFilterType")]
        [SwaggerSchema("Date filter type")]
        public DateFilterType DateFilterType { get; set; }
    }
}
