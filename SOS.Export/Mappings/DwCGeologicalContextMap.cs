using CsvHelper.Configuration;
using SOS.Export.Models.DarwinCore;

namespace SOS.Export.Mappings
{
    /// <summary>
    /// Mapping of Darwin Core to csv
    /// </summary>
    public class DwCGeologicalContextMap : ClassMap<DwCGeologicalContext>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DwCGeologicalContextMap()
        {
            Map(m => m.CoreID).Index(0).Name("coreID");
            Map(m => m.GeologicalContextID).Index(1).Name("geologicalContextID");
            Map(m => m.EarliestEonOrLowestEonothem).Index(2).Name("earliestEonOrLowestEonothem");
            Map(m => m.LatestEonOrHighestEonothem).Index(3).Name("latestEonOrHighestEonothem");
            Map(m => m.EarliestEraOrLowestErathem).Index(4).Name("earliestEraOrLowestErathem");
            Map(m => m.LatestEraOrHighestErathem).Index(5).Name("latestEraOrHighestErathem");
            Map(m => m.EarliestPeriodOrLowestSystem).Index(6).Name("earliestPeriodOrLowestSystem");
            Map(m => m.LatestPeriodOrHighestSystem).Index(7).Name("latestPeriodOrHighestSystem");
            Map(m => m.EarliestEpochOrLowestSeries).Index(8).Name("earliestEpochOrLowestSeries");
            Map(m => m.LatestEpochOrHighestSeries).Index(9).Name("latestEpochOrHighestSeries");
            Map(m => m.EarliestAgeOrLowestStage).Index(10).Name("earliestAgeOrLowestStage");
            Map(m => m.LatestAgeOrHighestStage).Index(11).Name("latestAgeOrHighestStage");
            Map(m => m.LowestBiostratigraphicZone).Index(12).Name("lowestBiostratigraphicZone");
            Map(m => m.HighestBiostratigraphicZone).Index(13).Name("highestBiostratigraphicZone");
            Map(m => m.LithostratigraphicTerms).Index(14).Name("lithostratigraphicTerms");
            Map(m => m.Group).Index(15).Name("group");
            Map(m => m.Formation).Index(16).Name("formation");
            Map(m => m.Member).Index(17).Name("member");
            Map(m => m.Bed).Index(18).Name("bed");
        }
    }
}