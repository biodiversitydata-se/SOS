namespace SOS.Lib.Models.Shared
{
    public class TaxonListTaxonInformation
    {
        public int Id { get; set; }
        public string ScientificName { get; set; }
        public string SwedishName { get; set; }

        public int? SensitivityCategory { get; set; }
    }
}