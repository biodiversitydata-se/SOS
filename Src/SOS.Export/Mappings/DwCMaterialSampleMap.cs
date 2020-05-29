using CsvHelper.Configuration;
using SOS.Export.Models.DarwinCore;

namespace SOS.Export.Mappings
{
    /// <summary>
    ///     Mapping of Darwin Core to csv
    /// </summary>
    public class DwCMaterialSampleMap : ClassMap<DwCMaterialSample>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public DwCMaterialSampleMap()
        {
            Map(m => m.CoreID).Index(0).Name("coreId");
            Map(m => m.MaterialSampleID).Index(1).Name("materialSampleID");
        }
    }
}