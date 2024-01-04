using Nest;
using System;

namespace SOS.Administration.Gui.Models
{
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class PerformanceResult
    {
        public int? Id { get; set; }
        public int TestId { get; set; }
        public long TimeTakenMs { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
