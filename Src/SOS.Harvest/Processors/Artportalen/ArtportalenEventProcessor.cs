using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Lib.Configuration.Process;
using DnsClient.Internal;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Processed.DataStewardship.Common;
using SOS.Lib.Models.Processed.DataStewardship.Enums;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Extensions;

namespace SOS.Harvest.Processors.Artportalen
{
    /// <summary>
    ///     Artportalen event processor.
    /// </summary>
    public class ArtportalenEventProcessor : EventProcessorBase<ArtportalenEventProcessor, ArtportalenObservationVerbatim, IVerbatimRepositoryBase<ArtportalenObservationVerbatim, int>>,
        IArtportalenEventProcessor
    {
        private readonly IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int> _artportalenVerbatimRepository;
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        public override DataProviderType Type => DataProviderType.ArtportalenObservations;

        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="processManager"></param>
        /// <param name="processTimeManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArtportalenEventProcessor(
            //IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int> artportalenVerbatimRepository,
            //IArtportalenVerbatimRepository artportalenVerbatimRepository // observation verbatim repository
            IProcessedObservationCoreRepository processedObservationRepository,            
            IObservationEventRepository observationEventRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<ArtportalenEventProcessor> logger) :
                base(observationEventRepository, processManager, processTimeManager, processConfiguration, logger)
        {
            _processedObservationRepository = processedObservationRepository;            
        }

        protected override async Task<int> ProcessEventsAsync(DataProvider dataProvider, IJobCancellationToken cancellationToken)
        {            
            int nrAddedEvents = await AddObservationEventsAsync(dataProvider);
            return nrAddedEvents;

            //var eventFactory = new ArtportalenEventFactory(dataProvider, TimeManager, ProcessConfiguration);
            //return await base.ProcessEventsAsync(
            //    dataProvider,
            //    eventFactory,
            //    null, //IArtportalenVerbatimRepository artportalenVerbatimRepository
            //    cancellationToken);
        }

        private async Task<int> AddObservationEventsAsync(DataProvider dataProvider)
        {
            try
            {
                Logger.LogInformation("Start AddObservationEventsAsync()");                
                int batchSize = 5000;
                var filter = new SearchFilter(0);
                filter.IsPartOfDataStewardshipDataset = true;
                filter.DataProviderIds = new List<int> { 1 };
                Logger.LogInformation($"AddObservationEventsAsync(). Read data from Observation index: {_processedObservationRepository.PublicIndexName}");
                var eventOccurrenceIds = await _processedObservationRepository.GetEventOccurrenceItemsAsync(filter);
                Dictionary<string, List<string>> totalOccurrenceIdsByEventId = eventOccurrenceIds.ToDictionary(m => m.EventId, m => m.OccurrenceIds);
                var chunks = totalOccurrenceIdsByEventId.Chunk(batchSize);
                int eventCount = 0;

                foreach (var chunk in chunks) // todo - do this step in parallel
                {
                    var occurrenceIdsByEventId = chunk.ToDictionary(m => m.Key, m => m.Value);
                    var firstOccurrenceIdInEvents = occurrenceIdsByEventId.Select(m => m.Value.First());
                    var observations = await _processedObservationRepository.GetObservationsAsync(firstOccurrenceIdInEvents, _observationEventOutputFields, false);
                    var events = new List<ObservationEvent>();
                    foreach (var observation in observations)
                    {
                        var occurrenceIds = occurrenceIdsByEventId[observation.Event.EventId.ToLower()];
                        var eventModel = observation.ToObservationEvent(occurrenceIds);
                        events.Add(eventModel);
                    }

                    // write to ES
                    eventCount += await ValidateAndStoreEvents(dataProvider, events, "");                    
                }

                Logger.LogInformation("End AddObservationEventsAsync()");
                return eventCount;                
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Add data stewardship events failed.");
                return 0;
            }
        }

        private readonly List<string> _observationEventOutputFields = new List<string>()
        {
            "occurrence",
            "location",
            "event",
            "dataStewardshipDatasetId",
            "institutionCode",
        };

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