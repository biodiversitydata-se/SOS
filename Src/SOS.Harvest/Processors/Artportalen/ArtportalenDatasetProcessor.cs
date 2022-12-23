using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Common;
using SOS.Lib.Models.Processed.DataStewardship.Enums;
using SOS.Lib.Models.Search.Filters;
using System.Data;
using DnsClient.Internal;

namespace SOS.Harvest.Processors.Artportalen
{
    /// <summary>
    ///     Artportalen dataset processor.
    /// </summary>
    public class ArtportalenDatasetProcessor : DatasetProcessorBase<ArtportalenDatasetProcessor, DwcVerbatimObservationDataset, IVerbatimRepositoryBase<DwcVerbatimObservationDataset, int>>,
        IArtportalenDatasetProcessor
    {
        //private readonly IVerbatimClient _verbatimClient;
        //private readonly IVerbatimRepositoryBase<DwcVerbatimObservationDataset, int> _artportalenVerbatimDatasetRepository;
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        public override DataProviderType Type => DataProviderType.ArtportalenObservations;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="processedDatasetsRepository"></param>
        /// <param name="processManager"></param>
        /// <param name="processTimeManager"></param>        
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArtportalenDatasetProcessor(
            //IVerbatimRepositoryBase<DwcVerbatimObservationDataset, int> artportalenVerbatimDatasetRepository,
            //IVerbatimClient verbatimClient,
            IProcessedObservationCoreRepository processedObservationRepository,            
            IObservationDatasetRepository processedDatasetsRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<ArtportalenDatasetProcessor> logger) :
                base(processedDatasetsRepository, processManager, processTimeManager, processConfiguration, logger)
        {
            _processedObservationRepository = processedObservationRepository;
        }

        /// <inheritdoc />
        protected override async Task<int> ProcessDatasetsAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            int nrAddedDatasets = await AddObservationDatasetsAsync(dataProvider);
            return nrAddedDatasets;
        }

        private async Task<ObservationDataset> GetSampleDatasetWithEventIdsAsync()
        {
            var dataset = GetSampleBatDataset();
            dataset.EventIds = await GetEventIdsAsync(dataset.Identifier);
            return dataset;
        }

        private async Task<List<string>?> GetEventIdsAsync(string datasetIdentifier)
        {
            var searchFilter = new SearchFilter(0);
            searchFilter.DataStewardshipDatasetIds = new List<string> { datasetIdentifier };
            var eventIds = await _processedObservationRepository.GetAllAggregationItemsAsync(searchFilter, "event.eventId");
            return eventIds?.Select(m => m.AggregationKey).ToList();
        }

        private async Task<int> AddObservationDatasetsAsync(DataProvider dataProvider)
        {
            try
            {                
                Logger.LogInformation("Start AddObservationDatasetsAsync()");                
                List<ObservationDataset> datasets = new List<ObservationDataset>();
                var batDataset = await GetSampleDatasetWithEventIdsAsync();
                datasets.Add(batDataset);
                int datasetCount = await ValidateAndStoreDatasets(dataProvider, datasets, "");                
                Logger.LogInformation("End AddObservationDatasetsAsync()");
                return datasetCount;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Add data stewardship datasets failed.");
                return 0;
            }
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
                Creator = new List<Organisation> { 
                    new Organisation
                    {
                        OrganisationID = "OrganisationId-unknown",
                        OrganisationCode = "SLU Artdatabanken"
                    }
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
                EndDate = DateTime.Now,
                Description = "Inventeringar av fladdermöss som görs inom det gemensamma delprogrammet för fladdermöss, dvs inom regional miljöövervakning, biogeografisk uppföljning och områdesvis uppföljning (uppföljning av skyddade områden).\r\n\r\nDet finns totalt tre projekt på Artportalen för det gemensamma delprogrammet och i detta projekt rapporteras data från den biogeografiska uppföljningen. Syftet med övervakningen är att följa upp hur antal och utbredning av olika arter förändras över tid. Övervakningen ger viktig information till bland annat EU-rapporteringar, rödlistningsarbetet och kan även användas i uppföljning av miljömålen och som underlag i ärendehandläggning. Den biogeografiska uppföljningen omfattar för närvarande några av de mest artrika fladdermuslokalerna i de olika biogeografiska regionerna i Sverige. Dessa inventeras vartannat år. Ett fåartsområde för fransfladdermus i norra Sverige samt några övervintringslokaler ingår också i övervakningen.",
                Title = "Fladdermöss",
                Spatial = "Sverige",
                Project = new List<Project> { 
                    new Project
                    {
                        ProjectId = "Artportalen ProjectId:3606",
                        ProjectCode = "Fladdermöss - gemensamt delprogram (biogeografisk uppföljning)",
                        ProjectType = ProjectType.Artportalenprojekt
                    }
                },                
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