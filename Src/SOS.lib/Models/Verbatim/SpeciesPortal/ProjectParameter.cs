
namespace SOS.Lib.Models.Verbatim.SpeciesPortal
{
    /// <summary>
    /// Project parameter.
    /// </summary>
    public class ProjectParameter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
    }
}