using SOS.DataStewardship.Api.Contracts.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    [SwaggerSchema("Date filter")]
    public class DateFilter
    {        
        [SwaggerSchema("Event start date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed")]
        public DateTime? StartDate { get; set; }
        
        [SwaggerSchema("Event end date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed")]
        public DateTime? EndDate { get; set; }
        
        [SwaggerSchema("Date filter type")]
        public DateFilterType DateFilterType { get; set; }
    }
}
