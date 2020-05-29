namespace SOS.Export.Models
{
    public class FieldDescription
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DwcIdentifier { get; set; }
        public string Class { get; set; }
        public int Importance { get; set; }
        public bool IncludedByDefaultInDwcExport { get; set; }
        public bool IsDwC { get; set; }
    }
}