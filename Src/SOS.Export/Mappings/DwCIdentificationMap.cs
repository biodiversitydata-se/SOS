using CsvHelper.Configuration;
using SOS.Export.Models.DarwinCore;

namespace SOS.Export.Mappings
{
    /// <summary>
    ///     Mapping of Darwin Core to csv
    /// </summary>
    public class DwCIdentificationMap : ClassMap<DwCIdentification>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public DwCIdentificationMap()
        {
            Map(m => m.CoreID).Index(0).Name("coreID");
            Map(m => m.IdentificationID).Index(1).Name("identificationID");
            Map(m => m.IdentificationQualifier).Index(2).Name("identificationQualifier");
            Map(m => m.TypeStatus).Index(3).Name("typeStatus");
            Map(m => m.IdentifiedBy).Index(4).Name("identifiedBy");
            Map(m => m.DateIdentified).Index(5).Name("dateIdentified");
            Map(m => m.IdentificationReferences).Index(6).Name("identificationReferences");
            Map(m => m.IdentificationVerificationStatus).Index(7).Name("identificationVerificationStatus");
            Map(m => m.IdentificationRemarks).Index(8).Name("identificationRemarks");
        }
    }
}