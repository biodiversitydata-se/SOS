using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Process
{
    public class TaxonServiceConfiguration : RestServiceConfiguration
    {
        public string TaxonApiAddress { get; set; }
        public bool UseTaxonApi { get; set; }
    }
}