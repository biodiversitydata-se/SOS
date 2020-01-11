using CsvHelper.Configuration;
using SOS.Export.Models.DarwinCore;

namespace SOS.Export.Mappings
{
    /// <summary>
    /// Mapping of Darwin Core to csv
    /// </summary>
    public class DwCMap : ClassMap<DwC>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DwCMap()
        {
            Map(m => m.DatasetID).Index(0).Name("datasetID");
            Map(m => m.Type).Index(1).Name("type");
            Map(m => m.Modified).Index(1).Name("modified");
            Map(m => m.Language).Index(1).Name("language");
            Map(m => m.License).Index(1).Name("license");
            Map(m => m.Type).Index(1).Name("rightsHolder");
            Map(m => m.RightsHolder).Index(1).Name("accessRights");
            Map(m => m.BibliographicCitation).Index(1).Name("bibliographicCitation");
            Map(m => m.References).Index(1).Name("references");
            Map(m => m.InstitutionID).Index(1).Name("institutionID");
            Map(m => m.CollectionID).Index(1).Name("collectionID");
            Map(m => m.InstitutionCode).Index(1).Name("institutionCode");
            Map(m => m.CollectionCode).Index(1).Name("collectionCode");
            Map(m => m.DatasetName).Index(1).Name("datasetName");
            Map(m => m.OwnerInstitutionCode).Index(1).Name("ownerInstitutionCode");
            Map(m => m.BasisOfRecord).Index(1).Name("basisOfRecord");
            Map(m => m.InformationWithheld).Index(1).Name("informationWithheld");
            Map(m => m.DataGeneralizations).Index(1).Name("dataGeneralizations");
            Map(m => m.DynamicProperties).Index(1).Name("dynamicProperties");
        }
    }
}
