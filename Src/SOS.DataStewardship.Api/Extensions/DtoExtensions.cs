using SOS.DataStewardship.Api.Models;
using SOS.DataStewardship.Api.Models.Enums;
using SOS.Lib.Enums.VocabularyValues;
using System.Data;
using ProcessedDataStewardship = SOS.Lib.Models.Processed.DataStewardship;

namespace SOS.DataStewardship.Api.Extensions
{
    public static class DtoExtensions
    {
        public static List<Dataset> ToDatasets(this IEnumerable<ProcessedDataStewardship.Dataset.ObservationDataset> datasets)
        {
            if (datasets == null || !datasets.Any()) return null;             
            return datasets.Select(m => m.ToDataset()).ToList();
        }

        public static Dataset ToDataset(this ProcessedDataStewardship.Dataset.ObservationDataset dataset)
        {
            if (dataset == null) return null;
                            
            return new Dataset
            {
                AccessRights = dataset.AccessRights.ToDatasetAccessRightsEnum(),
                Assigner = dataset.Assigner.ToOrganisation(),
                Creator = new List<Organisation> { dataset.Creator.ToOrganisation() }, // todo - support list of creators in ES index.
                DataStewardship = dataset.DataStewardship,
                Description = dataset.Description,
                EndDate = dataset.EndDate,
                EventIds = dataset.EventIds,
                Identifier = dataset.Identifier,
                Language = dataset.Language,
                Metadatalanguage = dataset.Metadatalanguage,
                Methodology = dataset.Methodology.ToMethodologies(),
                OwnerinstitutionCode = dataset.OwnerinstitutionCode.ToOrganisation(),
                ProjectCode = dataset.ProjectCode,
                ProjectID = dataset.ProjectId,
                Publisher = dataset.Publisher.ToOrganisation(),
                Purpose = dataset.Purpose.ToDatasetPurposeEnum(),
                Spatial = dataset.Spatial,
                StartDate = dataset.StartDate,
                Title = dataset.Title
            };
        }

        public static Purpose? ToDatasetPurposeEnum(this ProcessedDataStewardship.Enums.Purpose? purposeEnum)
        {
            if (purposeEnum == null) return null;
            return (Purpose)purposeEnum;
        }

        public static AccessRights? ToDatasetAccessRightsEnum(this ProcessedDataStewardship.Enums.AccessRights? accessRightsEnum)
        {
            if (accessRightsEnum == null) return null;
            return (AccessRights)accessRightsEnum;
        }

        public static Organisation ToOrganisation(this ProcessedDataStewardship.Common.Organisation organisation)
        {
            if (organisation == null) return null;
            return new Organisation
            {
                OrganisationID = organisation.OrganisationID,
                OrganisationCode = organisation.OrganisationCode
            };
        }

        public static List<Methodology> ToMethodologies(this IEnumerable<ProcessedDataStewardship.Dataset.Methodology> methodologies)
        {
            if (methodologies == null || !methodologies.Any()) return null;
            return methodologies.Select(m => m.ToMethodology()).ToList();
        }

        public static Methodology ToMethodology(this ProcessedDataStewardship.Dataset.Methodology methodology)
        {
            if (methodology == null) return null;
            return new Methodology
            {
                MethodologyDescription = methodology.MethodologyDescription,
                MethodologyLink = methodology.MethodologyLink,
                MethodologyName = methodology.MethodologyName,
                SpeciesList = methodology.SpeciesList
            };
        }

        public static EventModel ToEventModel(this ProcessedDataStewardship.Event.ObservationEvent observationEvent)
        {
            if (observationEvent == null) return null;

            var ev = new EventModel();
            ev.EventID = observationEvent.EventId;
            ev.ParentEventID = observationEvent.ParentEventId;
            ev.EventRemarks = observationEvent.EventRemarks;
            ev.AssociatedMedia = observationEvent.Media.ToAssociatedMedias();
            ev.Dataset = observationEvent?.Dataset.ToEventDataset();            
            ev.EventStartDate = observationEvent.StartDate;
            ev.EventEndDate = observationEvent.EndDate;
            ev.SamplingProtocol = observationEvent.SamplingProtocol;
            ev.SurveyLocation = observationEvent?.Location?.ToLocation();
            //ev.LocationProtected = ?
            //ev.EventType = ?
            //ev.Weather = ?
            ev.RecorderCode = observationEvent.RecorderCode;
            ev.RecorderOrganisation = observationEvent?.RecorderOrganisation?.Select(m => m.ToOrganisation()).ToList();

            ev.OccurrenceIds = observationEvent?.OccurrenceIds;
            ev.NoObservations = ev.OccurrenceIds == null || !ev.OccurrenceIds.Any();

            return ev;
        }       

        public static EventDataset ToEventDataset(this SOS.Lib.Models.Processed.DataStewardship.Event.EventDataset source)
        {
            if (source == null) return null;
            return new EventDataset
            {
                Identifier = source.Identifier,
                Title = source.Title,
            };
        }

        public static EventModel ToEventModel(this Observation observation, IEnumerable<string> occurrenceIds)
        {
            if (observation == null) return null;            
            var ev = new EventModel();
            ev.EventID = observation.Event.EventId;
            ev.ParentEventID = observation.Event.ParentEventId;
            ev.EventRemarks = observation.Event.EventRemarks;
            ev.AssociatedMedia = observation.Event.Media.ToAssociatedMedias();
            ev.Dataset = new EventDataset
            {
                Identifier = observation.DataStewardshipDatasetId,
                //Title = // need to lookup this from ObservationDataset index or store this information in Observation/Event
            };

            ev.EventStartDate = observation.Event.StartDate;
            ev.EventEndDate = observation.Event.EndDate;
            ev.SamplingProtocol = observation.Event.SamplingProtocol;
            ev.SurveyLocation = observation.Location.ToLocation();
            //ev.LocationProtected = ?
            //ev.EventType = ?
            //ev.Weather = ?
            ev.RecorderCode = new List<string>
            {
                observation.Occurrence.RecordedBy
            };
            if (observation?.InstitutionCode?.Value != null || !string.IsNullOrEmpty(observation.InstitutionId))
            {
                ev.RecorderOrganisation = new List<Organisation>
                {
                    new Organisation
                    {
                        OrganisationID = observation?.InstitutionId,
                        OrganisationCode = observation?.InstitutionCode?.Value
                    }
                };
            }

            ev.OccurrenceIds = occurrenceIds?.ToList();
            ev.NoObservations = ev.OccurrenceIds == null || !ev.OccurrenceIds.Any();

            return ev;
        }

        public static Models.Location ToLocation(this Lib.Models.Processed.Observation.Location location)
        {
            County? county = location?.County?.FeatureId?.GetCounty();

            return new Models.Location()
            {
                County = county.Value,
                //Province = 
                //Municipality =
                //Parish =
                Locality = location?.Locality,
                LocationID = location?.LocationId,
                LocationRemarks = location.LocationRemarks,
                //LocationType = // ? todo - add location type to models.
                Emplacement = location?.Point, // todo - decide if to use Point or PointWithBuffer
                EmplacementTest = new GeometryObject
                {
                    Type = "point",
                    Coordinates = new double[] { location.Point.Coordinates.Longitude, location.Point.Coordinates.Latitude }
                }
            };
        }

        public static List<AssociatedMedia> ToAssociatedMedias(this IEnumerable<Multimedia> multimedias)
        {
            if (multimedias == null || !multimedias.Any()) return null;
            return multimedias.Select(m => m.ToAssociatedMedia()).ToList();
        }

        public static AssociatedMedia ToAssociatedMedia(this Multimedia multimedia)
        {
            if (multimedia == null) return null;
            return new AssociatedMedia
            {
                AssociatedMediaName = multimedia.Title,
                AssociatedMediaType = GetAssociatedMediaTypeEnum(multimedia.Format),
                AssociatedMediaLink = multimedia.Identifier,
                License = multimedia.License,
                RightsHolder = multimedia.RightsHolder
            };
        }

        public static AssociatedMediaType GetAssociatedMediaTypeEnum(string format)
        {
            if (string.IsNullOrEmpty(format)) return AssociatedMediaType.Bild; // default
            string formatLower = format.ToLower();
            if (formatLower.StartsWith("image"))
                return AssociatedMediaType.Bild;
            if (formatLower.StartsWith("pdf"))
                return AssociatedMediaType.Pdf;
            if (formatLower.StartsWith("audio"))
                return AssociatedMediaType.Ljud;
            if (formatLower.StartsWith("video"))
                return AssociatedMediaType.Film;

            return AssociatedMediaType.Bild; // default
        }

        public static OccurrenceModel ToOccurrenceModel(this Observation observation)
        {
            var occurrence = new OccurrenceModel();
            occurrence.AssociatedMedia = observation.Occurrence?.Media.ToAssociatedMedias();
            if (observation?.BasisOfRecord?.Id != null)
            {
                occurrence.BasisOfRecord = GetBasisOfRecordEnum((BasisOfRecordId)observation.BasisOfRecord.Id);
            }

            occurrence.EventID = observation.Event.EventId;
            occurrence.DatasetIdentifier = observation.DataStewardshipDatasetId;
            occurrence.IdentificationVerificationStatus = IdentificationVerificationStatus.VärdelistaSaknas; // todo - implement when the value list is defined
            occurrence.ObservationCertainty = observation?.Location?.CoordinateUncertaintyInMeters == null ? null : Convert.ToDecimal(observation.Location.CoordinateUncertaintyInMeters);
            occurrence.ObservationPoint = observation?.Location?.Point;
            occurrence.ObservationPointTest = observation?.Location?.Point?.Coordinates == null ? null : new GeometryObject
            {
                Type = "point",
                Coordinates = new double[] { observation.Location.Point.Coordinates.Longitude, observation.Location.Point.Coordinates.Latitude }
            };
            occurrence.EventStartDate = observation.Event.StartDate;
            occurrence.EventEndDate = observation.Event.EndDate;
            occurrence.ObservationTime = observation.Event.StartDate == observation.Event.EndDate ? observation.Event.StartDate : null;            
            occurrence.OccurrenceID = observation.Occurrence.OccurrenceId;
            occurrence.OccurrenceRemarks = observation.Occurrence.OccurrenceRemarks;
            occurrence.OccurrenceStatus = observation.Occurrence.IsPositiveObservation ? OccurrenceStatus.Observerad : OccurrenceStatus.InteObserverad;
            occurrence.Quantity = Convert.ToDecimal(observation.Occurrence.OrganismQuantityInt);
            if (observation?.Occurrence?.OrganismQuantityUnit?.Id != null)
            {
                occurrence.QuantityVariable = GetQuantityVariableEnum((UnitId)observation.Occurrence.OrganismQuantityUnit.Id);
            }
            occurrence.Taxon = observation?.Taxon?.ToTaxonModel();
            //occurrence.Unit = ?
            occurrence.Organism = new OrganismVariable
            {
                Sex = observation?.Occurrence?.Sex?.Id == null ? null : GetSexEnum((SexId)observation.Occurrence.Sex.Id),
                Activity = observation?.Occurrence?.Activity?.Id == null ? null : GetActivityEnum((ActivityId)observation.Occurrence.Activity.Id),
                LifeStage = observation?.Occurrence?.LifeStage?.Id == null ? null : GetLifeStageEnum((LifeStageId)observation.Occurrence.LifeStage.Id),
            };

            return occurrence;            
        }

        public static BasisOfRecord GetBasisOfRecordEnum(BasisOfRecordId? basisOfRecordId)
        {
            switch(basisOfRecordId)
            {                
                case BasisOfRecordId.HumanObservation:
                    return BasisOfRecord.MänskligObservation;
                case BasisOfRecordId.MachineObservation:
                    return BasisOfRecord.MaskinellObservation;
                case BasisOfRecordId.MaterialSample:
                    return BasisOfRecord.FysisktProv;
                default:
                    return BasisOfRecord.Okänt;
            }
        }

        public static QuantityVariable? GetQuantityVariableEnum(UnitId unitId)
        {
            switch(unitId)
            {
                case UnitId.Individuals:
                    return QuantityVariable.AntalIndivider;
                case UnitId.Fruitbodies:
                    return QuantityVariable.AntalFruktkroppar;
                case UnitId.Capsules:
                    return QuantityVariable.AntalKapslar;
                case UnitId.Plants:
                    return QuantityVariable.AntalPlantorTuvor;
                case UnitId.Stems:
                    return QuantityVariable.AntalStjälkarStrånSkott;
                case UnitId.EggClusters:
                    return QuantityVariable.AntalÄggklumpar;
                //case UnitId.: // todo - add Täckningsgrad unit to SOS
                //    return OccurrenceModel.QuantityVariableEnum.Täckningsgrad;
                //case UnitId.: // todo - add Yttäckning unit to SOS
                //    return OccurrenceModel.QuantityVariableEnum.Yttäckning;
                default:
                    return null;
            }
        }

        public static TaxonModel ToTaxonModel(this Taxon taxon)
        {
            return new TaxonModel
            {
                ScientificName = taxon.ScientificName,
                TaxonID = taxon.Id.ToString(),
                TaxonRank = taxon.TaxonRank,
                VernacularName = taxon.VernacularName
                //VerbatimName = ? // todo - add support in SOS
                //VerbatimTaxonID = ?, todo - add support in SOS
            };
        }

        public static Sex? GetSexEnum(SexId? sexId)
        {
            if (sexId == null) return null;

            switch(sexId.Value)
            {
                case SexId.Male:
                    return Sex.Hane;
                case SexId.Female:
                    return Sex.Hona;
                case SexId.InPair:
                    return Sex.IPar;
                default:
                    return null;
            }
        }

        public static Activity? GetActivityEnum(ActivityId? activityId)
        {
            if (activityId == null) return null;

            switch (activityId.Value)
            {
                case ActivityId.FoundDead:
                    return Activity.Död;
                // todo - add mappings
                default:
                    return null;
            }
        }

        public static LifeStage? GetLifeStageEnum(LifeStageId? lifeStageId)
        {
            if (lifeStageId == null) return null;

            switch (lifeStageId.Value)
            {
                case LifeStageId.Adult:
                    return LifeStage.Adult;
                // todo - add mappings
                default:
                    return null;
            }
        }

        public static SearchFilter ToSearchFilter(this DatasetFilter datasetFilter)
        {
            if (datasetFilter == null) return null;

            var filter = new SearchFilter(0);
            filter.DataStewardshipDatasetIds = datasetFilter.DatasetList;
            filter.IsPartOfDataStewardshipDataset = true;
            if (datasetFilter.Taxon?.Ids != null && datasetFilter.Taxon.Ids.Any())
            {
                filter.Taxa = new Lib.Models.Search.Filters.TaxonFilter
                {
                    Ids = datasetFilter.Taxon.Ids,
                    IncludeUnderlyingTaxa = false
                };
            }

            if (datasetFilter.Datum != null)
            {
                filter.Date = new Lib.Models.Search.Filters.DateFilter
                {
                    StartDate = datasetFilter.Datum.StartDate,
                    EndDate = datasetFilter.Datum.EndDate,
                    DateFilterType = datasetFilter.Datum.DatumFilterType.ToDateRangeFilterType()
                };
            }

            filter.Location = datasetFilter.Area?.ToLocationFilter();

            return filter;
        }

        public static SearchFilter ToSearchFilter(this EventsFilter eventsFilter)
        {
            if (eventsFilter == null) return null;

            var filter = new SearchFilter(0);
            filter.DataStewardshipDatasetIds = eventsFilter.DatasetList;
            filter.IsPartOfDataStewardshipDataset = true;
            if (eventsFilter.Taxon?.Ids != null && eventsFilter.Taxon.Ids.Any())
            {
                filter.Taxa = new Lib.Models.Search.Filters.TaxonFilter
                {
                    Ids = eventsFilter.Taxon.Ids,
                    IncludeUnderlyingTaxa = false
                };
            }

            if (eventsFilter.Datum != null)
            {
                filter.Date = new Lib.Models.Search.Filters.DateFilter
                {
                    StartDate = eventsFilter.Datum.StartDate,
                    EndDate = eventsFilter.Datum.EndDate,
                    DateFilterType = eventsFilter.Datum.DatumFilterType.ToDateRangeFilterType()
                };
            }

            filter.Location = eventsFilter.Area?.ToLocationFilter();

            return filter;
        }

        public static SearchFilter ToSearchFilter(this OccurrenceFilter occurrenceFilter)
        {
            if (occurrenceFilter == null) return null;

            var filter = new SearchFilter(0);
            filter.EventIds = occurrenceFilter.EventIds;
            filter.DataStewardshipDatasetIds = occurrenceFilter.DatasetIds;
            filter.IsPartOfDataStewardshipDataset = true;
            if (occurrenceFilter.Taxon?.Ids != null && occurrenceFilter.Taxon.Ids.Any())
            {
                filter.Taxa = new Lib.Models.Search.Filters.TaxonFilter
                {
                    Ids = occurrenceFilter.Taxon.Ids,
                    IncludeUnderlyingTaxa = false
                };
            }
            
            if (occurrenceFilter.Datum != null)
            {
                filter.Date = new Lib.Models.Search.Filters.DateFilter
                {
                    StartDate = occurrenceFilter.Datum.StartDate,
                    EndDate = occurrenceFilter.Datum.EndDate,
                    DateFilterType = occurrenceFilter.Datum.DatumFilterType.ToDateRangeFilterType()
                };                
            }
            
            filter.Location = occurrenceFilter.Area?.ToLocationFilter();

            return filter;
        }

        public static LocationFilter ToLocationFilter(this SOS.DataStewardship.Api.Models.GeographicsFilter geographicsFilter)
        {
            if (geographicsFilter == null) return null;
            var locationFilter = new LocationFilter();
            var areaFilter = new List<AreaFilter>();

            // County
            if (geographicsFilter.County != null)
            {
                areaFilter.Add(new AreaFilter {
                    AreaType = AreaType.County,
                    FeatureId = geographicsFilter.County.Value.GetCountyFeatureId()
                });                                        
            }

            // Municipality - todo


            locationFilter.Geometries = geographicsFilter.Area?.ToGeographicsFilter();
            locationFilter.Areas = areaFilter;
            return locationFilter;
        }

        public static SOS.Lib.Models.Search.Filters.GeographicsFilter ToGeographicsFilter(this SOS.DataStewardship.Api.Models.GeographicsFilterArea geographicsFilterArea)
        {
            if (geographicsFilterArea == null) return null;
            var geographicsFilter = new SOS.Lib.Models.Search.Filters.GeographicsFilter();
            geographicsFilter.MaxDistanceFromPoint = geographicsFilterArea.MaxDistanceFromGeometries;
            if (geographicsFilterArea.GeographicArea != null)
            {
                geographicsFilter.Geometries = new List<IGeoShape> { geographicsFilterArea.GeographicArea }; // todo - change filter type to List<IGeoShape>?
            }

            return geographicsFilter;
        }


        public static Lib.Models.Search.Filters.DateFilter.DateRangeFilterType ToDateRangeFilterType(this DatumFilterType datumFilterType)
        {
            switch (datumFilterType)
            {
                case DatumFilterType.OnlyStartDate:
                    return Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.OnlyStartDate;
                case DatumFilterType.OnlyEndDate:
                    return Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.OnlyEndDate;
                case DatumFilterType.OverlappingStartDateAndEndDate:
                    return Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate;
                case DatumFilterType.BetweenStartDateAndEndDate:
                    return Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.BetweenStartDateAndEndDate;
                default:
                    return Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate;
            }
        }
    }
}