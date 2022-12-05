using SOS.Lib.Models.Shared;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.DataStewardship.Common;
using SOS.Lib.Models.Processed.DataStewardship.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Harvest.Processors.Artportalen
{
    /// <summary>
    ///     Artportalen dataset factory.
    /// </summary>
    public class ArtportalenDatasetFactory : DatasetFactoryBase, IDatasetFactory<DwcVerbatimObservationDataset>
    {
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>        
        public ArtportalenDatasetFactory(
            DataProvider dataProvider,            
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration) : base(dataProvider, processTimeManager, processConfiguration)
        {
            
        }

        public ObservationDataset CreateProcessedDataset(DwcVerbatimObservationDataset verbatimDataset)
        {
            try
            {
                if (verbatimDataset == null)
                {
                    return null;
                }

                var id = $"urn:lsid:{DataProvider.ChecklistIdentifier}:Dataset:{verbatimDataset.Identifier}";
                
                var observationDataset = new ObservationDataset
                {
                    Id = id, // verbatimDataset.Identifier,
                    AccessRights = verbatimDataset.AccessRights,
                    Assigner = verbatimDataset.Assigner,
                    Creator = verbatimDataset.Creator,
                    DataStewardship = verbatimDataset.DataStewardship,
                    Description = verbatimDataset.Description,
                    EndDate = verbatimDataset.EndDate,
                    EventIds = verbatimDataset.EventIds,                    
                    Identifier = verbatimDataset.Identifier,
                    Language = verbatimDataset.Language,
                    Metadatalanguage = verbatimDataset.Metadatalanguage,
                    Methodology = verbatimDataset.Methodology,
                    OwnerinstitutionCode = verbatimDataset.OwnerinstitutionCode,
                    ProjectCode = verbatimDataset.ProjectCode,
                    ProjectId = verbatimDataset.ProjectId,
                    Publisher = verbatimDataset.Publisher,
                    Purpose = verbatimDataset.Purpose,
                    Spatial = verbatimDataset.Spatial,
                    StartDate = verbatimDataset.StartDate,
                    Title = verbatimDataset.Title
                };

                return observationDataset;
            }
            catch (Exception e)
            {
                throw new Exception($"Error when processing DwC verbatim dataset with Identifier={verbatimDataset.Identifier ?? "null"}", e);
            }
        }

        private async Task<ObservationDataset> GetSampleDatasetWithEventIdsAsync()
        {
            var dataset = GetSampleBatDataset();
            dataset.EventIds = await GetEventIdsAsync(dataset.Identifier);
            return dataset;
        }

        private async Task<List<string>> GetEventIdsAsync(string datasetIdentifier)
        {
            var searchFilter = new SearchFilter(0);
            searchFilter.DataStewardshipDatasetIds = new List<string> { datasetIdentifier };
            var eventIds = await _processedObservationRepository.GetAllAggregationItemsAsync(searchFilter, "event.eventId");
            return eventIds?.Select(m => m.AggregationKey).ToList();
        }

        private ObservationDataset GetSampleBatDataset()
        {
            var dataset = new ObservationDataset
            {
                Identifier = "ArtportalenDataHost - Dataset Bats", // Ändra till Id-nummer. Enligt DPS:en ska det vara ett id-nummer från informationsägarens metadatakatalog. Om det är LST som är informationsägare så bör de ha datamängden registrerad i sin metadatakatalog, med ett id där.
                Metadatalanguage = "Swedish",
                Language = "Swedish",
                AccessRights = AccessRights.Publik,
                Purpose = Purpose.NationellMiljöövervakning,
                Assigner = new Organisation
                {
                    OrganisationID = "2021001975",
                    OrganisationCode = "Naturvårdsverket"
                },
                // Creator = Utförare av datainsamling.
                Creator = new Organisation
                {
                    OrganisationID = "OrganisationId-unknown",
                    OrganisationCode = "SLU Artdatabanken"
                },
                // Finns inte alltid i AP, behöver skapas/hämtasandra DV informationskällor?
                OwnerinstitutionCode = new Organisation
                {
                    OrganisationID = "OrganisationId-unknown",
                    OrganisationCode = "Länsstyrelsen Jönköping"
                },
                Publisher = new Organisation
                {
                    OrganisationID = "OrganisationId-unknown",
                    OrganisationCode = "SLU Artdatabanken"
                },
                DataStewardship = "Datavärdskap Naturdata: Arter",
                StartDate = new DateTime(2011, 1, 1),
                EndDate = null,
                Description = "Inventeringar av fladdermöss som görs inom det gemensamma delprogrammet för fladdermöss, dvs inom regional miljöövervakning, biogeografisk uppföljning och områdesvis uppföljning (uppföljning av skyddade områden).\r\n\r\nDet finns totalt tre projekt på Artportalen för det gemensamma delprogrammet och i detta projekt rapporteras data från den biogeografiska uppföljningen. Syftet med övervakningen är att följa upp hur antal och utbredning av olika arter förändras över tid. Övervakningen ger viktig information till bland annat EU-rapporteringar, rödlistningsarbetet och kan även användas i uppföljning av miljömålen och som underlag i ärendehandläggning. Den biogeografiska uppföljningen omfattar för närvarande några av de mest artrika fladdermuslokalerna i de olika biogeografiska regionerna i Sverige. Dessa inventeras vartannat år. Ett fåartsområde för fransfladdermus i norra Sverige samt några övervintringslokaler ingår också i övervakningen.",
                Title = "Fladdermöss",
                Spatial = "Sverige",
                ProjectId = "Artportalen ProjectId:3606",
                ProjectCode = "Fladdermöss - gemensamt delprogram (biogeografisk uppföljning)",
                Methodology = new List<Methodology>
                {
                    new Methodology
                    {
                        MethodologyDescription = "Methodology description?", // finns sällan i projektbeskrivning i AP, behöver hämtas från andra DV informationskällor
                        MethodologyLink = "https://www.naturvardsverket.se/upload/stod-i-miljoarbetet/vagledning/miljoovervakning/handledning/metoder/undersokningstyper/landskap/fladdermus-artkartering-2017-06-05.pdf",
                        MethodologyName = "Undersökningstyp fladdermöss - artkartering",
                        SpeciesList = "Species list?" // artlistan behöver skapas, alternativt "all species occurring in Sweden"
                    }
                },
                EventIds = new List<string>
                {
                    //"urn:lsid:artportalen.se:site:3775204#2012-03-06T08:00:00+01:00/2012-03-06T13:00:00+01:00"
                }
            };

            return dataset;
        }
    }
}