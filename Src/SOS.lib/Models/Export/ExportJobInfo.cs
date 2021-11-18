using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Models.Export
{
    public class ExportJobInfo : IEntity<string>
    {
        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ProcessStartDate { get; set; }
        public DateTime? ProcessEndDate { get; set; }
        public TimeSpan? ProcessingTime { get; set; }
        public int? NumberOfObservations { get; set; }
        public string Description { get; set; }
        public ExportFormat Format { get; set; }
        public ExportJobStatus Status { get; set; }
        public string ErrorMsg { get; set; }
        public OutputFieldSet? OutputFieldSet { get; set; }
    }
}
