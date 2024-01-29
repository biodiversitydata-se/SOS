
namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Artportalen project parameter.
    /// </summary>
    public class ProjectParameterValue : ProjectParameter
    {
        /// <summary>
        /// Value of the data in string format.
        /// </summary>
        public string Value { get; set; }

        public override string ToString()
        {
            return $"[{Name}={Value}]";
        }
    }
}