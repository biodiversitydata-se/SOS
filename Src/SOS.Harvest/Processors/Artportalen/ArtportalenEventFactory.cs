//using NetTopologySuite.Geometries;
//using SOS.Lib.Enums;
//using SOS.Lib.Extensions;
//using SOS.Lib.Models.Processed.Checklist;
//using SOS.Lib.Models.Processed.Observation;
//using SOS.Lib.Models.Shared;
//using SOS.Lib.Models.Verbatim.Artportalen;
//using SOS.Harvest.Managers.Interfaces;
//using SOS.Harvest.Processors.Interfaces;
//using Area = SOS.Lib.Models.Processed.Observation.Area;
//using Location = SOS.Lib.Models.Processed.Observation.Location;
//using SOS.Lib.Configuration.Process;
//using SOS.Lib.Models.Processed.DataStewardship.Dataset;
//using SOS.Lib.Models.Search.Filters;
//using SOS.Lib.Models.Processed.DataStewardship.Event;
//using Microsoft.Extensions.Logging;

//namespace SOS.Harvest.Processors.Artportalen
//{
//    public class ArtportalenEventFactory : IEventFactory<ArtportalenObservationVerbatim>
//    {
//        //private readonly ILogger<ArtportalenEventFactory> _logger;

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="dataProvider"></param>
//        public ArtportalenEventFactory(
//            DataProvider dataProvider,
//            IProcessTimeManager processTimeManager,
//            ProcessConfiguration processConfiguration)
//            //ILogger<ArtportalenEventFactory> logger) //: base(dataProvider, processTimeManager, processConfiguration)
//        {
//            //_logger = logger;
//        }

//        public ObservationEvent CreateEventObservation(ArtportalenObservationVerbatim verbatim)
//        {
//            throw new NotImplementedException();
//        }

//        private async Task AddObservationEventsAsync()
//        {
//            try
//            {
//                //_logger.LogInformation("Start AddObservationEventsAsync()");                
//                int batchSize = 5000;
//                var filter = new SearchFilter(0);
//                filter.IsPartOfDataStewardshipDataset = true;
//                //_logger.LogInformation($"AddObservationEventsAsync(). Read data from Observation index: {_processedObservationRepository.PublicIndexName}");
//                var eventOccurrenceIds = await _processedObservationRepository.GetEventOccurrenceItemsAsync(filter);
//                Dictionary<string, List<string>> totalOccurrenceIdsByEventId = eventOccurrenceIds.ToDictionary(m => m.EventId, m => m.OccurrenceIds);
//                var chunks = totalOccurrenceIdsByEventId.Chunk(batchSize);

//                foreach (var chunk in chunks) // todo - do this step in parallel
//                {
//                    var occurrenceIdsByEventId = chunk.ToDictionary(m => m.Key, m => m.Value);
//                    var firstOccurrenceIdInEvents = occurrenceIdsByEventId.Select(m => m.Value.First());
//                    var observations = await _processedObservationRepository.GetObservationsAsync(firstOccurrenceIdInEvents, _observationEventOutputFields, false);
//                    var events = new List<ObservationEvent>();
//                    foreach (var observation in observations)
//                    {
//                        var occurrenceIds = occurrenceIdsByEventId[observation.Event.EventId.ToLower()];
//                        var eventModel = observation.ToObservationEvent(occurrenceIds);
//                        events.Add(eventModel);
//                    }

//                    // write to ES
//                    await _observationEventRepository.AddManyAsync(events);
//                }
//                await EnableEsEventIndexingAsync();
//                //_logger.LogInformation("End AddObservationEventsAsync()");
//            }
//            catch (Exception e)
//            {
//                //_logger.LogError(e, "Add data stewardship events failed.");
//            }
//        }


//        //private async Task AddDatasetsAsync()
//        //{
//        //    try
//        //    {
//        //        _logger.LogInformation("Start AddDatasetsAsync()");
//        //        await InitializeElasticSearchDatasetAsync();
//        //        await DisableEsDatasetIndexingAsync();
//        //        List<Dataset> datasets = new List<Dataset>();
//        //        var batDataset = GetSampleBatDataset();
//        //        batDataset.EndDate = DateTime.Now;

//        //        // Determine which events that belongs to this dataset. Aggregate unique EventIds with filter: ProjectIds in [3606]
//        //        _logger.LogInformation($"AddDatasetsAsync(). Read data from Observation index: {_processedObservationRepository.PublicIndexName}");
//        //        var searchFilter = new SearchFilter(0);
//        //        searchFilter.DataStewardshipDatasetIds = new List<string> { batDataset.Identifier };
//        //        var eventIds = await _processedObservationRepository.GetAllAggregationItemsAsync(searchFilter, "event.eventId");
//        //        batDataset.EventIds = eventIds.Select(m => m.AggregationKey).ToList();
//        //        datasets.Add(batDataset);
//        //        await _observationDatasetRepository.AddManyAsync(datasets);
//        //        await EnableEsDatasetIndexingAsync();
//        //        _logger.LogInformation("End AddDatasetsAsync()");
//        //    }
//        //    catch (Exception e)
//        //    {
//        //        _logger.LogError(e, "Add data stewardship datasets failed.");
//        //    }
//        //}


//        ///// <summary>
//        ///// Cast verbatim area to processed area
//        ///// </summary>
//        ///// <param name="area"></param>
//        ///// <returns></returns>
//        //private static Area CastToArea(GeographicalArea area)
//        //{
//        //    if (area == null)
//        //    {
//        //        return null;
//        //    }

//        //    return new Area
//        //    {
//        //        FeatureId = area.FeatureId,
//        //        Name = area.Name
//        //    };
//        //}

//        //private Event CreateEvent(ArtportalenChecklistVerbatim verbatim, string id)
//        //{
//        //    var evnt = new Event(verbatim.StartDate, verbatim.EndDate)
//        //    {
//        //        EventId = id,
//        //        SamplingProtocol = verbatim.Project?.SurveyMethod ?? verbatim.Project?.SurveyMethodUrl,
//        //    };

//        //    return evnt;
//        //}

//        ///// <summary>
//        ///// Create location object
//        ///// </summary>
//        ///// <param name="verbatim"></param>
//        ///// <returns></returns>
//        //private Location CreateLocation(ArtportalenChecklistVerbatim verbatim)
//        //{
//        //    var location = new Location();

//        //    if (verbatim.Site == null)
//        //    {
//        //        location.Locality = verbatim.Name;
//        //        AddPositionData(location, verbatim.OccurrenceXCoord, verbatim.OccurrenceYCoord,
//        //            CoordinateSys.Rt90_25_gon_v, 0, 0);

//        //        return location;
//        //    }

//        //    var point = (Point)verbatim.Site?.Point?.ToGeometry()!;

//        //    var site = verbatim.Site;
//        //    location.Attributes.CountyPartIdByCoordinate = site.CountyPartIdByCoordinate;
//        //    location.Attributes.ProvincePartIdByCoordinate = site.ProvincePartIdByCoordinate;
//        //    location.CountryRegion = CastToArea(site?.CountryRegion!);
//        //    location.County = CastToArea(site?.County!);
//        //    location.Locality = site.Name.Trim();
//        //    location.LocationId = $"urn:lsid:artportalen.se:site:{site?.Id}";
//        //    location.Municipality = CastToArea(site?.Municipality!);
//        //    location.Parish = CastToArea(site?.Parish!);
//        //    location.Province = CastToArea(site?.Province!);
//        //    AddPositionData(location, site.XCoord,
//        //        site.YCoord,
//        //        CoordinateSys.WebMercator,
//        //        point,
//        //        site.PointWithBuffer,
//        //        site.Accuracy,
//        //        0);

//        //    return location;
//        //}



//        ///// <summary>
//        /////     Cast verbatim checklist to processed data model
//        ///// </summary>
//        ///// <param name="verbatimChecklist"></param>
//        ///// <returns></returns>
//        //public Checklist CreateProcessedChecklist(ArtportalenChecklistVerbatim verbatimChecklist)
//        //{
//        //    try
//        //    {
//        //        if (verbatimChecklist == null)
//        //        {
//        //            return null;
//        //        }

//        //        var id = $"urn:lsid:artportalen.se:Checklist:{verbatimChecklist.Id}";
//        //        return new Checklist
//        //        {
//        //            ArtportalenInternal = new ApInternal()
//        //            {
//        //                ChecklistId = verbatimChecklist.Id,
//        //                ParentTaxonId = verbatimChecklist.ParentTaxonId,
//        //                UserId = verbatimChecklist.ControllingUserId
//        //            },
//        //            DataProviderId = DataProvider.Id,
//        //            Id = id,
//        //            Event = CreateEvent(verbatimChecklist, id),
//        //            Location = CreateLocation(verbatimChecklist),
//        //            Modified = verbatimChecklist.EditDate,
//        //            Name = verbatimChecklist.Name,
//        //            OccurrenceIds =
//        //                verbatimChecklist.SightingIds?.Select(sId => $"urn:lsid:artportalen.se:Sighting:{sId}"),
//        //            Project = ArtportalenFactoryHelper.CreateProcessedProject(verbatimChecklist.Project),
//        //            RecordedBy = verbatimChecklist.ControllingUser,
//        //            RegisterDate = verbatimChecklist.RegisterDate,
//        //            TaxonIds = verbatimChecklist.TaxonIds,
//        //            TaxonIdsFound = verbatimChecklist.TaxonIdsFound
//        //        };
//        //    }
//        //    catch (Exception e)
//        //    {
//        //        throw new Exception($"Error when processing Artportalen verbatim checklist with Id={verbatimChecklist.Id}", e);
//        //    }
//        //}
//    }    
//}