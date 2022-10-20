using Microsoft.Extensions.Azure;
using SOS.DataStewardship.Api.Models;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Dataset;
using System.Data;

namespace SOS.DataStewardship.Api.Extensions
{
    public static class DtoExtensions
    {
        public static List<Dataset> ToDatasets(this IEnumerable<ObservationDataset> datasets)
        {
            if (datasets == null || !datasets.Any()) return null;             
            return datasets.Select(m => m.ToDataset()).ToList();
        }

        public static Dataset ToDataset(this ObservationDataset dataset)
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
                Events = dataset.EventIds,
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

        public static Dataset.PurposeEnum? ToDatasetPurposeEnum(this ObservationDataset.PurposeEnum? purposeEnum)
        {
            if (purposeEnum == null) return null;
            return (Dataset.PurposeEnum)purposeEnum;
        }

        public static Dataset.AccessRightsEnum? ToDatasetAccessRightsEnum(this ObservationDataset.AccessRightsEnum? accessRightsEnum)
        {
            if (accessRightsEnum == null) return null;
            return (Dataset.AccessRightsEnum)accessRightsEnum;
        }

        public static Organisation ToOrganisation(this ObservationDataset.Organisation organisation)
        {
            if (organisation == null) return null;
            return new Organisation
            {
                OrganisationID = organisation.OrganisationID,
                OrganisationCode = organisation.OrganisationCode
            };
        }

        public static List<Methodology> ToMethodologies(this IEnumerable<ObservationDataset.MethodologyModel> methodologies)
        {
            if (methodologies == null || !methodologies.Any()) return null;
            return methodologies.Select(m => m.ToMethodology()).ToList();
        }

        public static Methodology ToMethodology(this ObservationDataset.MethodologyModel methodology)
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
            ev.SurveyLocation = observation.ToLocation();
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

            ev.Occurrences = occurrenceIds.ToList();
            if (ev.Occurrences.Any())
            {
                ev.NoObservations = EventModel.NoObservationsEnum.Falskt;
            }
            else
            {
                ev.NoObservations = EventModel.NoObservationsEnum.Sant;
            }

            return ev;
        }

        public static Models.Location ToLocation(this Observation observation)
        {
            County? county = observation?.Location?.County?.FeatureId?.GetCounty();

            var location = new Models.Location()
            {
                County = county.Value,
                //Province = 
                //Municipality =
                //Parish =
                Locality = observation?.Location?.Locality,
                LocationID = observation?.Location?.LocationId,
                LocationRemarks = observation?.Location.LocationRemarks,
                //LocationType = // ? todo - add location type to models.
                Emplacement = observation?.Location?.Point, // todo - decide if to use Point or PointWithBuffer
                EmplacementTest = new GeometryObject
                {
                    Type = "point",
                    Coordinates = new double[] { observation.Location.Point.Coordinates.Longitude, observation.Location.Point.Coordinates.Latitude }
                }
            };

            return location;
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

        public static AssociatedMedia.AssociatedMediaTypeEnum GetAssociatedMediaTypeEnum(string format)
        {
            if (string.IsNullOrEmpty(format)) return AssociatedMedia.AssociatedMediaTypeEnum.Bild; // default
            string formatLower = format.ToLower();
            if (formatLower.StartsWith("image"))
                return AssociatedMedia.AssociatedMediaTypeEnum.Bild;
            if (formatLower.StartsWith("pdf"))
                return AssociatedMedia.AssociatedMediaTypeEnum.Pdf;
            if (formatLower.StartsWith("audio"))
                return AssociatedMedia.AssociatedMediaTypeEnum.Ljud;
            if (formatLower.StartsWith("video"))
                return AssociatedMedia.AssociatedMediaTypeEnum.Film;

            return AssociatedMedia.AssociatedMediaTypeEnum.Bild; // default
        }

        public static OccurrenceModel ToOccurrenceModel(this Observation observation)
        {
            var occurrence = new OccurrenceModel();
            occurrence.AssociatedMedia = observation.Occurrence?.Media.ToAssociatedMedias();
            if (observation?.BasisOfRecord?.Id != null)
            {
                occurrence.BasisOfRecord = GetBasisOfRecordEnum((BasisOfRecordId)observation.BasisOfRecord.Id);
            }

            occurrence.Event = observation.Event.EventId;
            occurrence.DatasetIdentifier = observation.DataStewardshipDatasetId;
            occurrence.IdentificationVerificationStatus = Models.OccurrenceModel.IdentificationVerificationStatusEnum.VärdelistaSaknas; // todo - implement when the value list is defined
            occurrence.ObservationCertainty = Convert.ToDecimal(observation.Location.CoordinateUncertaintyInMeters);
            occurrence.ObservationPoint = observation.Location.Point;
            occurrence.ObservationPointTest = new GeometryObject
            {
                Type = "point",
                Coordinates = new double[] { observation.Location.Point.Coordinates.Longitude, observation.Location.Point.Coordinates.Latitude }
            };
            occurrence.EventStartDate = observation.Event.StartDate;
            occurrence.EventEndDate = observation.Event.EndDate;
            occurrence.ObservationTime = observation.Event.StartDate == observation.Event.EndDate ? observation.Event.StartDate : null;            
            occurrence.OccurrenceID = observation.Occurrence.OccurrenceId;
            occurrence.OccurrenceRemarks = observation.Occurrence.OccurrenceRemarks;
            occurrence.OccurrenceStatus = observation.Occurrence.IsPositiveObservation ? Models.OccurrenceModel.OccurrenceStatusEnum.Observerad : Models.OccurrenceModel.OccurrenceStatusEnum.InteObserverad;
            occurrence.Quantity = Convert.ToDecimal(observation.Occurrence.OrganismQuantityInt);
            if (observation?.Occurrence?.OrganismQuantityUnit?.Id != null)
            {
                occurrence.QuantityVariable = GetQuantityVariableEnum((UnitId)observation.Occurrence.OrganismQuantityUnit.Id);
            }
            occurrence.Taxon = observation.Taxon.ToTaxonModel();
            //occurrence.Unit = ?
            occurrence.Organism = new OrganismVariable
            {
                Sex = observation?.Occurrence?.Sex?.Id == null ? null : GetSexEnum((SexId)observation.Occurrence.Sex.Id),
                Activity = observation?.Occurrence?.Activity?.Id == null ? null : GetActivityEnum((ActivityId)observation.Occurrence.Activity.Id),
                LifeStage = observation?.Occurrence?.LifeStage?.Id == null ? null : GetLifeStageEnum((LifeStageId)observation.Occurrence.LifeStage.Id),
            };

            return occurrence;            
        }

        public static OccurrenceModel.BasisOfRecordEnum GetBasisOfRecordEnum(BasisOfRecordId? basisOfRecordId)
        {
            switch(basisOfRecordId)
            {                
                case BasisOfRecordId.HumanObservation:
                    return OccurrenceModel.BasisOfRecordEnum.MänskligObservation;
                case BasisOfRecordId.MachineObservation:
                    return OccurrenceModel.BasisOfRecordEnum.MaskinellObservation;
                case BasisOfRecordId.MaterialSample:
                    return OccurrenceModel.BasisOfRecordEnum.FysisktProv;
                default:
                    return OccurrenceModel.BasisOfRecordEnum.Okänt;
            }
        }

        public static OccurrenceModel.QuantityVariableEnum? GetQuantityVariableEnum(UnitId unitId)
        {
            switch(unitId)
            {
                case UnitId.Individuals:
                    return OccurrenceModel.QuantityVariableEnum.AntalIndivider;
                case UnitId.Fruitbodies:
                    return OccurrenceModel.QuantityVariableEnum.AntalFruktkroppar;
                case UnitId.Capsules:
                    return OccurrenceModel.QuantityVariableEnum.AntalKapslar;
                case UnitId.Plants:
                    return OccurrenceModel.QuantityVariableEnum.AntalPlantorTuvor;
                case UnitId.Stems:
                    return OccurrenceModel.QuantityVariableEnum.AntalStjälkarStrånSkott;
                case UnitId.EggClusters:
                    return OccurrenceModel.QuantityVariableEnum.AntalÄggklumpar;
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

        public static OrganismVariable.SexEnum? GetSexEnum(SexId? sexId)
        {
            if (sexId == null) return null;

            switch(sexId.Value)
            {
                case SexId.Male:
                    return OrganismVariable.SexEnum.Hane;
                case SexId.Female:
                    return OrganismVariable.SexEnum.Hona;
                case SexId.InPair:
                    return OrganismVariable.SexEnum.IPar;
                default:
                    return null;
            }
        }

        public static OrganismVariable.ActivityEnum? GetActivityEnum(ActivityId? activityId)
        {
            if (activityId == null) return null;

            switch (activityId.Value)
            {
                case ActivityId.FoundDead:
                    return OrganismVariable.ActivityEnum.Död;
                // todo - add mappings
                default:
                    return null;
            }
        }

        public static OrganismVariable.LifeStageEnum? GetLifeStageEnum(LifeStageId? lifeStageId)
        {
            if (lifeStageId == null) return null;

            switch (lifeStageId.Value)
            {
                case LifeStageId.Adult:
                    return OrganismVariable.LifeStageEnum.Adult;
                // todo - add mappings
                default:
                    return null;
            }
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
                filter.Date = new DateFilter
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
            filter.DataStewardshipDatasetIds = occurrenceFilter.DatasetList;
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
                filter.Date = new DateFilter
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


        public static DateFilter.DateRangeFilterType ToDateRangeFilterType(this DatumFilterType datumFilterType)
        {
            switch (datumFilterType)
            {
                case DatumFilterType.OnlyStartDate:
                    return DateFilter.DateRangeFilterType.OnlyStartDate;
                case DatumFilterType.OnlyEndDate:
                    return DateFilter.DateRangeFilterType.OnlyEndDate;
                case DatumFilterType.OverlappingStartDateAndEndDate:
                    return DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate;
                case DatumFilterType.BetweenStartDateAndEndDate:
                    return DateFilter.DateRangeFilterType.BetweenStartDateAndEndDate;
                default:
                    return DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate;
            }
        }
    }
}