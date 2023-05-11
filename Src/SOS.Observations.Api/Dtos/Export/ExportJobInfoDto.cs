using System;
using SOS.Lib.Enums;
using SOS.Lib.Models.Export;

namespace SOS.Observations.Api.Dtos.Export
{
    /// <summary>
    /// Keep control of user exports
    /// </summary>
    public class ExportJobInfoDto
    {

        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public DateTime? ProcessStartDate { get; set; }
        public DateTime? ProcessEndDate { get; set; }
        public TimeSpan? ProcessingTime => ProcessEndDate.HasValue && ProcessStartDate.HasValue ? ProcessEndDate.Value - ProcessStartDate.Value : null;
        public int? NumberOfObservations { get; set; }
        public string Description { get; set; }

        public ExportFormat Format { get; set; }

        public ExportJobStatus Status { get; set; }

        public OutputFieldSet? OutputFieldSet { get; set; }
        public string PickUpUrl { get; set; }
    }
}
