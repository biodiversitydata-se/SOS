namespace SOS.Lib.Models.Processed.Sighting
{
    /// <summary>
    /// Darwin Core Project Parameter.
    /// </summary>
    public class ProjectParameter
    {
        //  "occurrenceID", "measurementType", "measurementValue", "measurementUnit" 

        public string OccurrenceId { get; set; }
        public int ProjectId { get; set; }
        public int ProjectParameterId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
    }
}
