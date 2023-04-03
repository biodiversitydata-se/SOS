using NetTopologySuite.Geometries;
using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.DataStewardship.Common;
using System.Data;
using ProcessedDataStewardship = SOS.Lib.Models.Processed.DataStewardship;

namespace SOS.DataStewardship.Api.Extensions
{
    public static class DtoExtensions
    {
        public static List<Dataset> ToDatasets(this IEnumerable<ProcessedDataStewardship.Dataset.Dataset> datasets)
        {
            if (datasets == null || !datasets.Any()) return null;             
            return datasets.Select(m => m.ToDataset()).ToList();
        }

        public static Dataset ToDataset(this ProcessedDataStewardship.Dataset.Dataset dataset)
        {
            if (dataset == null) return null;
                            
            return new Dataset
            {
                AccessRights = dataset.AccessRights.ToDatasetAccessRightsEnum(),
                DescriptionAccessRights= dataset.DescriptionAccessRights,
                Assigner = dataset.Assigner.ToOrganisation(),
                Creator = dataset?.Creator?.Select(m => m.ToOrganisation())?.ToList(),
                DataStewardship = dataset.DataStewardship,
                Description = dataset.Description,
                EndDate = dataset.EndDate,
                EventIds = dataset.EventIds,
                Identifier = dataset.Identifier,
                Language = dataset.Language,
                Metadatalanguage = dataset.Metadatalanguage,
                Methodology = dataset.Methodology.ToMethodologies(),
                OwnerinstitutionCode = dataset.OwnerinstitutionCode.ToOrganisation(),
                Project = dataset.Project?.Select(m => m.ToProject())?.ToList(),
                ProgrammeArea = dataset.ProgrammeArea.ToProgrammeAreaEnum(),
                Publisher = dataset.Publisher.ToOrganisation(),
                Purpose = dataset.Purpose.ToDatasetPurposeEnum(),
                Spatial = dataset.Spatial,
                StartDate = dataset.StartDate,
                Title = dataset.Title
            };
        }

        public static Contracts.Models.Project ToProject(this ProcessedDataStewardship.Common.Project project)
        {
            if (project == null) return null;
            return new Contracts.Models.Project
            {
                ProjectCode= project.ProjectCode,
                ProjectID = project.ProjectId,
                ProjectType = project.ProjectType == null ? null : (ProjectType)project.ProjectType
            };
        }

        public static Purpose? ToDatasetPurposeEnum(this ProcessedDataStewardship.Enums.Purpose? purposeEnum)
        {
            if (purposeEnum == null) return null;
            return (Purpose)purposeEnum;
        }

        public static ProgrammeArea? ToProgrammeAreaEnum(this ProcessedDataStewardship.Enums.ProgrammeArea? programmeArea)
        {
            if (programmeArea == null) return null;
            return (ProgrammeArea)programmeArea;
        }

        public static AccessRights? ToDatasetAccessRightsEnum(this ProcessedDataStewardship.Enums.AccessRights? accessRightsEnum)
        {
            if (accessRightsEnum == null) return null;
            return (AccessRights)accessRightsEnum;
        }

        public static SOS.DataStewardship.Api.Contracts.Models.Organisation ToOrganisation(this ProcessedDataStewardship.Common.Organisation organisation)
        {
            if (organisation == null) return null;
            return new SOS.DataStewardship.Api.Contracts.Models.Organisation
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

        public static EventModel ToEventModel(this ProcessedDataStewardship.Event.Event observationEvent, CoordinateSystem responseCoordinateSystem)
        {
            if (observationEvent == null) return null;

            var ev = new EventModel();
            ev.EventID = observationEvent.EventId;
            ev.ParentEventID = observationEvent.ParentEventId;
            ev.EventRemarks = observationEvent.EventRemarks;
            ev.AssociatedMedia = observationEvent.Media.ToAssociatedMedias();
            ev.Dataset = observationEvent?.DataStewardship.ToDatasetInfo();            
            ev.EventStartDate = observationEvent.StartDate;
            ev.EventEndDate = observationEvent.EndDate;
            ev.SamplingProtocol = observationEvent.SamplingProtocol;
            ev.SurveyLocation = observationEvent?.Location?.ToLocation(responseCoordinateSystem);
            ev.EventType = observationEvent?.EventType;
            ev.LocationProtected = observationEvent?.LocationProtected;
            ev.Weather = observationEvent?.Weather?.ToWeatherVariable();
            ev.RecorderCode = observationEvent.RecorderCode;
            ev.RecorderOrganisation = observationEvent?.RecorderOrganisation?.Select(m => m.ToOrganisation()).ToList();

            ev.OccurrenceIds = observationEvent?.OccurrenceIds;
            ev.NoObservations = ev.OccurrenceIds == null || !ev.OccurrenceIds.Any();

            return ev;
        }

        public static WeatherVariable ToWeatherVariable(this ProcessedDataStewardship.Event.WeatherVariable source)
        {            
            if (source == null) return null;

            return new WeatherVariable
            {

                SnowCover = source.SnowCover?.ToSnowCover(),
                Sunshine = source.Sunshine?.ToWeatherMeasuring(),
                AirTemperature = source.AirTemperature?.ToWeatherMeasuring(),
                WindDirectionCompass = source.WindDirectionCompass?.ToWindDirectionCompass(),
                WindDirectionDegrees = source.WindDirectionDegrees?.ToWeatherMeasuring(),
                WindSpeed = source.WindDirectionDegrees?.ToWeatherMeasuring(),
                WindStrength = source.WindStrength?.ToWindStrength(),
                Precipitation = source.Precipitation?.ToPrecipitation(),
                Visibility = source.Visibility?.ToVisibility(),
                Cloudiness = source.Cloudiness?.ToCloudiness()
            };
        }

        private static Cloudiness ToCloudiness(this ProcessedDataStewardship.Enums.Cloudiness source)
        {
            return (Cloudiness)source;
        }

        private static Visibility ToVisibility(this ProcessedDataStewardship.Enums.Visibility source)
        {
            return (Visibility)source;
        }

        private static Precipitation ToPrecipitation(this ProcessedDataStewardship.Enums.Precipitation source)
        {
            return (Precipitation)source;
        }

        private static WindStrength ToWindStrength(this ProcessedDataStewardship.Enums.WindStrength source)
        {
            return (WindStrength)source;
        }

        private static WindDirectionCompass ToWindDirectionCompass(this ProcessedDataStewardship.Enums.WindDirectionCompass source)
        {
            return (WindDirectionCompass)source;
        }        

        private static Contracts.Enums.Unit ToUnit(this ProcessedDataStewardship.Enums.Unit source)
        {
            return (Contracts.Enums.Unit)source;
        }

        private static SnowCover ToSnowCover(this ProcessedDataStewardship.Enums.SnowCover source)
        {            
            return (SnowCover)source;
        }

        public static WeatherMeasuring ToWeatherMeasuring(this ProcessedDataStewardship.Event.WeatherMeasuring source)
        {
            if (source == null) return null;

            return new WeatherMeasuring
            {
                Unit = source.Unit?.ToUnit(),
                WeatherMeasure = source.WeatherMeasure
            };
        }

        public static DatasetInfo ToDatasetInfo(this DataStewardshipInfo source)
        {
            if (source == null) return null;
            return new DatasetInfo
            {
                Identifier = source.DatasetIdentifier,
                Title = source.DatasetTitle,
            };
        }

        public static EventModel ToEventModel(this Observation observation, IEnumerable<string> occurrenceIds, CoordinateSystem responseCoordinateSystem)
        {
            if (observation == null) return null;            
            var ev = new EventModel();
            ev.EventID = observation.Event.EventId;
            ev.ParentEventID = observation.Event.ParentEventId;
            ev.EventRemarks = observation.Event.EventRemarks;
            ev.AssociatedMedia = observation.Event.Media.ToAssociatedMedias();
            ev.Dataset = new DatasetInfo
            {
                Identifier = observation.DataStewardship?.DatasetIdentifier
                //Title = // need to lookup this from ObservationDataset index or store this information in Observation/Event
            };

            ev.EventStartDate = observation.Event.StartDate;
            ev.EventEndDate = observation.Event.EndDate;
            ev.SamplingProtocol = observation.Event.SamplingProtocol;
            ev.SurveyLocation = observation.Location.ToLocation(responseCoordinateSystem);            
            //ev.LocationProtected = ?
            //ev.EventType = ?
            //ev.Weather = ?
            ev.RecorderCode = new List<string>
            {
                observation.Occurrence.RecordedBy
            };
            if (observation?.InstitutionCode?.Value != null || !string.IsNullOrEmpty(observation.InstitutionId))
            {
                ev.RecorderOrganisation = new List<SOS.DataStewardship.Api.Contracts.Models.Organisation>
                {
                    new SOS.DataStewardship.Api.Contracts.Models.Organisation
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

        public static Contracts.Models.Location ToLocation(this Lib.Models.Processed.Observation.Location location, CoordinateSystem responseCoordinateSystem)
        {
            County? county = location?.County?.FeatureId?.GetCounty();
            Municipality? municipality = location?.Municipality?.FeatureId?.GetMunicipality();
            Parish? parish = location?.Parish?.FeatureId?.GetParish();
            Province? province = location?.Province?.FeatureId?.GetProvince();

            return new Contracts.Models.Location()
            {
                County = county.Value,
                Province = province.Value,
                Municipality = municipality.Value,
                Parish = parish.Value,
                Locality = location?.Locality,
                LocationID = location?.LocationId,
                LocationRemarks = location.LocationRemarks,
                //LocationType = // ? todo - add location type to models.
                Emplacement = location?.Point.ConvertCoordinateSystem(responseCoordinateSystem), // todo - decide if to use Point or PointWithBuffer                
            };
        }

        public static IGeoShape ConvertCoordinateSystem(this PointGeoShape point, CoordinateSystem responseCoordinateSystem)
        {
            if (point == null) return null;

            var pointToTransform = new Point(point.Coordinates.Longitude, point.Coordinates.Latitude);            
            var targetCoordinateSys = responseCoordinateSystem switch
            {
                CoordinateSystem.EPSG3006 => CoordinateSys.SWEREF99_TM,
                CoordinateSystem.EPSG3857 => CoordinateSys.WebMercator,
                CoordinateSystem.EPSG4258 => CoordinateSys.ETRS89,
                CoordinateSystem.EPSG4326 => CoordinateSys.WGS84,
                CoordinateSystem.EPSG4619 => CoordinateSys.SWEREF99,
                _ => throw new Exception($"Not handled coordinate system {responseCoordinateSystem}")
            };
            
            var transformedPoint = pointToTransform.Transform(CoordinateSys.WGS84, targetCoordinateSys);
            return transformedPoint.ToGeoShape();            
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

        public static OccurrenceModel ToOccurrenceModel(this Observation observation, CoordinateSystem responseCoordinateSystem)
        {
            var occurrence = new OccurrenceModel();
            occurrence.AssociatedMedia = observation.Occurrence?.Media.ToAssociatedMedias();
            if (observation?.BasisOfRecord?.Id != null)
            {
                occurrence.BasisOfRecord = GetBasisOfRecordEnum((BasisOfRecordId)observation.BasisOfRecord.Id);
            }

            occurrence.EventID = observation.Event.EventId;
            occurrence.Dataset ??= new DatasetInfo();
            occurrence.Dataset.Identifier = observation?.DataStewardship?.DatasetIdentifier;
            occurrence.IdentificationVerificationStatus = observation?.Identification?.VerificationStatus?.Value;
            occurrence.ObservationCertainty = observation?.Location?.CoordinateUncertaintyInMeters == null ? null : Convert.ToDouble(observation.Location.CoordinateUncertaintyInMeters);
            occurrence.ObservationPoint = observation?.Location?.Point.ConvertCoordinateSystem(responseCoordinateSystem);
            occurrence.EventStartDate = observation.Event.StartDate;
            occurrence.EventEndDate = observation.Event.EndDate;
            occurrence.ObservationTime = observation.Event.StartDate == observation.Event.EndDate ? observation.Event.StartDate : null;            
            occurrence.OccurrenceID = observation.Occurrence.OccurrenceId;
            occurrence.OccurrenceRemarks = observation.Occurrence.OccurrenceRemarks;
            occurrence.OccurrenceStatus = observation.Occurrence.IsPositiveObservation ? OccurrenceStatus.Observerad : OccurrenceStatus.InteObserverad;
            occurrence.Quantity = Convert.ToDouble(observation.Occurrence.OrganismQuantityInt);
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
            return basisOfRecordId switch
            {
                BasisOfRecordId.HumanObservation => BasisOfRecord.MänskligObservation,
                BasisOfRecordId.MachineObservation => BasisOfRecord.MaskinellObservation,
                BasisOfRecordId.MaterialSample => BasisOfRecord.FysisktProv,
                _ =>  BasisOfRecord.Okänt
            };
        }
        
        public static QuantityVariable? GetQuantityVariableEnum(UnitId unitId)
        {
            return unitId switch
            {
                UnitId.CoverClass => QuantityVariable.Yttäckning,
                UnitId.CoverPercentage => QuantityVariable.Täckningsgrad,
                UnitId.Individuals => QuantityVariable.AntalIndivider,
                UnitId.Fruitbodies => QuantityVariable.AntalFruktkroppar,
                UnitId.Capsules => QuantityVariable.AntalKapslar,
                UnitId.Plants => QuantityVariable.AntalPlantorTuvor,
                UnitId.Stems => QuantityVariable.AntalStjälkarStrånSkott,
                UnitId.EggClusters => QuantityVariable.AntalÄggklumpar,
                _ => null
            };
        }

        public static TaxonModel ToTaxonModel(this Taxon taxon)
        {
            return new TaxonModel
            {
                ScientificName = taxon.ScientificName,
                TaxonID = taxon.Id.ToString(),
                TaxonRank = taxon.TaxonRank,
                VernacularName = taxon.VernacularName,
                VerbatimTaxonID = taxon.VerbatimId,
                VerbatimName = taxon.VerbatimName
            };
        }

        public static Sex? GetSexEnum(SexId? sexId)
        {
            if (sexId == null) return null;

            return sexId.Value switch
            {
                SexId.Male => Sex.Hane,
                SexId.Female => Sex.Hona,
                SexId.InPair => Sex.IPar,
               _ => null!
            };
        }

        public static Activity? GetActivityEnum(ActivityId? activityId)
        {
            if (activityId == null) return null;

            return activityId.Value switch
            {
                ActivityId.AgitatedBehaviour => Activity.UpprördVarnande,
                ActivityId.BreedingFailed => Activity.MisslyckadHäckning,
                ActivityId.BuildingNestOrUsedNestOrNest => Activity.Bobygge,
                ActivityId.Call => Activity.LockläteÖvrigaLäten,
                ActivityId.CarryingFoodForYoung => Activity.FödaÅtUngar,
                ActivityId.DeadCollidedWithAeroplane => Activity.Död,
                ActivityId.DeadCollidedWithFence => Activity.Död,
                ActivityId.DeadCollidedWithLighthouse => Activity.Död,
                ActivityId.DeadCollidedWithPowerLine => Activity.Död,
                ActivityId.DeadCollidedWithWindMill => Activity.Död,
                ActivityId.DeadCollidedWithWindow => Activity.Död,
                ActivityId.DeadDueToDiseaseOrStarvation => Activity.Död,
                ActivityId.Display => Activity.SpelSång,
                ActivityId.DisplayOrSong => Activity.SpelSång,
                ActivityId.DisplayOrSongOutsideBreeding => Activity.SpelSångEjHäckning,
                ActivityId.DisputeBetweenMales => Activity.Revirhävdande,
                ActivityId.DistractionDisplay => Activity.AvledningsbeteendeEnum,
                ActivityId.Dormant => Activity.Vilande,
                ActivityId.EggLaying => Activity.Äggläggande,
                ActivityId.EggShells => Activity.Äggskal,
                ActivityId.FlyingOverhead => Activity.Förbiflygande,
                ActivityId.Foraging => Activity.Födosökande,
                ActivityId.FoundDead => Activity.Död,
                ActivityId.Fragment => Activity.Fragment,
                ActivityId.Freeflying => Activity.Friflygande,
                ActivityId.FreshGnaw => Activity.FärskaGnagspår,
                ActivityId.Gnaw => Activity.ÄldreGnagspår,
                ActivityId.Incubating => Activity.Ruvande,
                ActivityId.InNestingHabitat => Activity.ObsIHäcktidLämpligBiotop,
                ActivityId.InWater => Activity.IVattenSimmande,
                ActivityId.InWaterOrSwimming => Activity.IVattenSimmande,
                ActivityId.KilledByElectricity => Activity.Död,
                ActivityId.KilledByOil => Activity.Död,
                ActivityId.KilledByPredator => Activity.Död,
                ActivityId.MatingOrMatingCeremonies => Activity.Parning,
                ActivityId.Migrating => Activity.Sträckande,
                ActivityId.MigratingE => Activity.Sträckande,
                ActivityId.MigratingFish => Activity.Sträckande,
                ActivityId.MigratingN => Activity.Sträckande,
                ActivityId.MigratingNE => Activity.Sträckande,
                ActivityId.MigratingNW => Activity.Sträckande,
                ActivityId.MigratingS => Activity.Sträckande,
                ActivityId.MigratingSE => Activity.Sträckande,
                ActivityId.MigratingSW => Activity.Sträckande,
                ActivityId.MigratingW => Activity.Sträckande,
                ActivityId.NestBuilding => Activity.Bobygge,
                ActivityId.NestWithChickHeard => Activity.BoHördaUngar,
                ActivityId.NestWithEgg => Activity.BoÄggUngar,
                ActivityId.OldNest => Activity.AnväntBo,
                ActivityId.PairInSuitableHabitat => Activity.ParILämpligHäckbiotop,
                ActivityId.PermanentTerritory => Activity.PermanentRevir,
                ActivityId.PregnantFemale => Activity.DräktigHona,
                ActivityId.RecentlyFledgedYoung => Activity.PulliNyligenFlyggaUngar,
                ActivityId.RecentlyUsedNest => Activity.AnväntBo,
                ActivityId.RoadKill => Activity.Död,
                ActivityId.RunningOrCrawling => Activity.FrispringandeKrypande,
                ActivityId.ShotOrKilled => Activity.Död,
                ActivityId.SignsOfGnawing => Activity.FärskaGnagspår,
                ActivityId.Staging => Activity.Rastande,
                ActivityId.Stationary => Activity.Stationär,
                ActivityId.Territorial => Activity.Revirhävdande,
                ActivityId.TerritoryOutsideBreeding => Activity.Revirhävdande,
                ActivityId.VisitingOccupiedNest => Activity.BesökerBebottBo,
                ActivityId.VisitPossibleNest => Activity.Bobesök,
                ActivityId.WinterHabitat => Activity.PåÖvervintringsplats,
                _ => null
            };
        }

        public static LifeStage? GetLifeStageEnum(LifeStageId? lifeStageId)
        {
            if (lifeStageId == null) return null;

            return lifeStageId switch
            {
                LifeStageId.Adult => LifeStage.Adult,
                LifeStageId.AtLeast1StCalendarYear => LifeStage._1KPlus,
                LifeStageId.AtLeast2NdCalendarYear => LifeStage._2KPlus,
                LifeStageId.AtLeast3RdCalendarYear => LifeStage._3KPlus,
                LifeStageId.AtLeast4ThCalendarYear => LifeStage._4KPlus,
                LifeStageId.BudBurst => LifeStage.Knoppbristning,
                LifeStageId.Cub => LifeStage.Årsunge,
                LifeStageId.Egg => LifeStage.Ägg,
                LifeStageId.FirstCalendarYear => LifeStage._1K,
                LifeStageId.Flowering => LifeStage.Blomning,
                LifeStageId.FourthCalendarYear => LifeStage._4K,
                LifeStageId.FourthCalendarYearOrYounger => LifeStage._4KMinus,
                LifeStageId.FullyDevelopedLeaf => LifeStage.FulltUtveckladeBlad,
                LifeStageId.ImagoOrAdult => LifeStage.ImagoAdult,
                LifeStageId.Juvenile => LifeStage.Juvenil,
                LifeStageId.Larvae => LifeStage.Larv,
                LifeStageId.LarvaOrNymph => LifeStage.LarvNymf,
                LifeStageId.LeafCutting => LifeStage.GulnadeLövBlad,
                LifeStageId.Nestling => LifeStage.Pulli,
                LifeStageId.OnePlus => LifeStage._1KPlus,
                LifeStageId.Overblown => LifeStage.Överblommad,
                LifeStageId.Pupa => LifeStage.Puppa,
                LifeStageId.Rest => LifeStage.Vilstadium,
                LifeStageId.SecondCalendarYear => LifeStage._2K,
                LifeStageId.Sprout => LifeStage.MedGroddkorn,
                LifeStageId.ThirdCalendarYear => LifeStage._3K,
                LifeStageId.ThirdCalendarYearOrYounger => LifeStage._3KMinus,
                LifeStageId.WithCapsule => LifeStage.MedKapsel,
                LifeStageId.WithFemaleParts => LifeStage.MedHonorgan,
                LifeStageId.WithoutCapsule => LifeStage.UtanKapsel,
                LifeStageId.YellowingLeaves => LifeStage.GulnadeLövBlad,
                LifeStageId.ZeroPlus => LifeStage.Årsyngel,
                _ => null
            };
        }

        public static SearchFilter ToSearchFilter(this DatasetFilter datasetFilter)
        {
            if (datasetFilter == null) return null;

            var filter = new SearchFilter(0);
            filter.DataStewardshipDatasetIds = datasetFilter.DatasetIds ?? datasetFilter.DatasetList;
            filter.IsPartOfDataStewardshipDataset = true;
            if (datasetFilter.Taxon?.Ids != null && datasetFilter.Taxon.Ids.Any())
            {
                filter.Taxa = new Lib.Models.Search.Filters.TaxonFilter
                {
                    Ids = datasetFilter.Taxon.Ids,
                    IncludeUnderlyingTaxa = false
                };
            }

            if (datasetFilter.DateFilter != null)
            {
                filter.Date = new Lib.Models.Search.Filters.DateFilter
                {
                    StartDate = datasetFilter.DateFilter.StartDate,
                    EndDate = datasetFilter.DateFilter.EndDate,
                    DateFilterType = datasetFilter.DateFilter.DateFilterType.ToDateRangeFilterType()
                };
            }
            else if (datasetFilter.Datum != null)
            {
                filter.Date = new Lib.Models.Search.Filters.DateFilter
                {
                    StartDate = datasetFilter.Datum.StartDate,
                    EndDate = datasetFilter.Datum.EndDate,
                    DateFilterType = datasetFilter.Datum.DateFilterType.ToDateRangeFilterType()
                };
            }

            filter.Location = datasetFilter.Area?.ToLocationFilter();

            return filter;
        }

        public static SearchFilter ToSearchFilter(this EventsFilter eventsFilter)
        {
            if (eventsFilter == null) return null;

            var filter = new SearchFilter(0);
            filter.DataStewardshipDatasetIds = eventsFilter.DatasetIds ?? eventsFilter.DatasetList;
            filter.EventIds = eventsFilter.EventIds;
            filter.IsPartOfDataStewardshipDataset = true;
            if (eventsFilter.Taxon?.Ids?.Any() ?? false)
            {
                filter.Taxa = new Lib.Models.Search.Filters.TaxonFilter
                {
                    Ids = eventsFilter.Taxon.Ids,
                    IncludeUnderlyingTaxa = false
                };
            }

            if (eventsFilter.DateFilter != null)
            {
                filter.Date = new Lib.Models.Search.Filters.DateFilter
                {
                    StartDate = eventsFilter.DateFilter.StartDate,
                    EndDate = eventsFilter.DateFilter.EndDate,
                    DateFilterType = eventsFilter.DateFilter.DateFilterType.ToDateRangeFilterType()
                };
            }
            else if (eventsFilter.Datum != null)
            {
                filter.Date = new Lib.Models.Search.Filters.DateFilter
                {
                    StartDate = eventsFilter.Datum.StartDate,
                    EndDate = eventsFilter.Datum.EndDate,
                    DateFilterType = eventsFilter.Datum.DateFilterType.ToDateRangeFilterType()
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
            filter.DataStewardshipDatasetIds = occurrenceFilter.DatasetIds ?? occurrenceFilter.DatasetList;
            filter.IsPartOfDataStewardshipDataset = true;
            if (occurrenceFilter.Taxon?.Ids != null && occurrenceFilter.Taxon.Ids.Any())
            {
                filter.Taxa = new Lib.Models.Search.Filters.TaxonFilter
                {
                    Ids = occurrenceFilter.Taxon.Ids,
                    IncludeUnderlyingTaxa = false
                };
            }
            
            if (occurrenceFilter.DateFilter != null)
            {
                filter.Date = new Lib.Models.Search.Filters.DateFilter
                {
                    StartDate = occurrenceFilter.DateFilter.StartDate,
                    EndDate = occurrenceFilter.DateFilter.EndDate,
                    DateFilterType = occurrenceFilter.DateFilter.DateFilterType.ToDateRangeFilterType()
                };                
            }
            else if (occurrenceFilter.Datum != null)
            {
                filter.Date = new Lib.Models.Search.Filters.DateFilter
                {
                    StartDate = occurrenceFilter.Datum.StartDate,
                    EndDate = occurrenceFilter.Datum.EndDate,
                    DateFilterType = occurrenceFilter.Datum.DateFilterType.ToDateRangeFilterType()
                };
            }

            filter.Location = occurrenceFilter.Area?.ToLocationFilter();

            return filter;
        }

        public static LocationFilter ToLocationFilter(this Contracts.Models.GeographicsFilter geographicsFilter)
        {
            if (geographicsFilter == null) return null;
            var locationFilter = new LocationFilter();
            var areaFilter = new List<AreaFilter>();

            // County
            if (geographicsFilter.County != null)
            {
                areaFilter.Add(new AreaFilter {
                    AreaType = AreaType.County,
                    FeatureId = ((int)geographicsFilter.County.Value).ToString()
                });                                        
            }

            if (geographicsFilter.Municipality != null)
            {
                areaFilter.Add(new AreaFilter
                {
                    AreaType = AreaType.Municipality,
                    FeatureId = ((int)geographicsFilter.Municipality.Value).ToString()
                });
            }

            if (geographicsFilter.Parish != null)
            {
                areaFilter.Add(new AreaFilter
                {
                    AreaType = AreaType.Parish,
                    FeatureId = ((int)geographicsFilter.Parish.Value).ToString()
                });
            }

            if (geographicsFilter.Province != null)
            {
                areaFilter.Add(new AreaFilter
                {
                    AreaType = AreaType.Province,
                    FeatureId = ((int)geographicsFilter.Province.Value).ToString()
                });
            }

            locationFilter.Geometries = geographicsFilter.Geometry?.ToGeographicsFilter() ?? geographicsFilter.Area?.ToGeographicsFilter();
            locationFilter.Areas = areaFilter;
            return locationFilter;
        }

        public static SOS.Lib.Models.Search.Filters.GeographicsFilter ToGeographicsFilter(this GeometryFilter geographicsFilterArea)
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


        public static Lib.Models.Search.Filters.DateFilter.DateRangeFilterType ToDateRangeFilterType(this DateFilterType dateFilterType)
        {
            return dateFilterType switch
            {
                DateFilterType.OnlyStartDate => Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.OnlyStartDate,
                DateFilterType.OnlyEndDate => Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.OnlyEndDate,
                DateFilterType.OverlappingStartDateAndEndDate => Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate,
                DateFilterType.BetweenStartDateAndEndDate => Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.BetweenStartDateAndEndDate,
                _ =>  Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate
            };
        }
    }
}