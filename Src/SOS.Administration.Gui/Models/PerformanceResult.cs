namespace SOS.Administration.Gui.Models
{
    public class PerformanceResult
    {
        public int? Id { get; set; }
        public int TestId { get; set; }
        public long TimeTakenMs { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
