
namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Darwin Core Project Parameter.
    /// </summary>
    public class ProcessedProjectParameter
    {
        //  "occurrenceID", "measurementType", "measurementValue", "measurementUnit" 

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
    }
}
