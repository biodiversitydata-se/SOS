using NetTopologySuite.Geometries;
using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.DataStewardship.Common;
using System.Data;
using ProcessedDataStewardship = SOS.Lib.Models.Processed.DataStewardship;

namespace SOS.DataStewardship.Api.Extensions;

public static class DtoExtensions
{
    extension(IEnumerable<ProcessedDataStewardship.Dataset.Dataset> datasets)
    {
        public List<Dataset> ToDatasets()
        {
            if (datasets == null || !datasets.Any()) return null;
            return datasets.Select(m => m.ToDataset()).ToList();
        }
    }

    extension(ProcessedDataStewardship.Dataset.Dataset dataset)
    {
        public Dataset ToDataset()
        {
            if (dataset == null) return null;

            return new Dataset
            {
                AccessRights = dataset.AccessRights.ToDatasetAccessRightsEnum(),
                DescriptionAccessRights = dataset.DescriptionAccessRights,
                Assigner = dataset.Assigner.ToOrganisation(),
                Creator = dataset?.Creator?.Select(m => m.ToOrganisation())?.ToList(),
                DataStewardship = dataset.DataStewardship,
                Description = dataset.Description,
                EndDate = dataset.EndDate?.ToLocalTime(),
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
                StartDate = dataset.StartDate?.ToLocalTime(),
                Title = dataset.Title
            };
        }
    }

    extension(ProcessedDataStewardship.Common.Project project)
    {
        public Contracts.Models.Project ToProject()
        {
            if (project == null) return null;
            return new Contracts.Models.Project
            {
                ProjectCode = project.ProjectCode,
                ProjectID = project.ProjectId,
                ProjectType = project.ProjectType == null ? null : (ProjectType)project.ProjectType
            };
        }
    }

    extension(ProcessedDataStewardship.Enums.Purpose? purposeEnum)
    {
        public Purpose? ToDatasetPurposeEnum()
        {
            if (purposeEnum == null) return null;
            return (Purpose)purposeEnum;
        }
    }

    extension(ProcessedDataStewardship.Enums.ProgrammeArea? programmeArea)
    {
        public ProgrammeArea? ToProgrammeAreaEnum()
        {
            if (programmeArea == null) return null;
            return (ProgrammeArea)programmeArea;
        }
    }

    extension(ProcessedDataStewardship.Enums.AccessRights? accessRightsEnum)
    {
        public AccessRights? ToDatasetAccessRightsEnum()
        {
            if (accessRightsEnum == null) return null;
            return (AccessRights)accessRightsEnum;
        }
    }

    extension(ProcessedDataStewardship.Common.Organisation organisation)
    {
        public SOS.DataStewardship.Api.Contracts.Models.Organisation ToOrganisation()
        {
            if (organisation == null) return null;
            return new SOS.DataStewardship.Api.Contracts.Models.Organisation
            {
                OrganisationID = organisation.OrganisationID,
                OrganisationCode = organisation.OrganisationCode
            };
        }
    }

    extension(IEnumerable<ProcessedDataStewardship.Dataset.Methodology> methodologies)
    {
        public List<Methodology> ToMethodologies()
        {
            if (methodologies == null || !methodologies.Any()) return null;
            return methodologies.Select(m => m.ToMethodology()).ToList();
        }
    }

    extension(ProcessedDataStewardship.Dataset.Methodology methodology)
    {
        public Methodology ToMethodology()
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
    }

    extension(ProcessedDataStewardship.Event.Event observationEvent)
    {
        public Contracts.Models.Event ToEventModel(CoordinateSystem responseCoordinateSystem)
        {
            if (observationEvent == null) return null;

            GetEventDates(
                observationEvent.StartDate.Value,
                observationEvent.EndDate.Value,
                observationEvent.PlainStartDate,
                observationEvent.PlainEndDate,
                observationEvent.PlainStartTime,
                observationEvent.PlainEndTime,
                out DateOnly startDate,
                out DateOnly endDate,
                out TimeOnly? startTime,
                out TimeOnly? endTime);

            var ev = new Contracts.Models.Event();
            ev.EventID = observationEvent.EventId;
            ev.ParentEventID = observationEvent.ParentEventId;
            ev.EventRemarks = observationEvent.EventRemarks;
            ev.AssociatedMedia = observationEvent.Media?.ToAssociatedMedias();
            ev.Dataset = observationEvent?.DataStewardship?.ToDatasetInfo();
            ev.EventStartDateTime = observationEvent.StartDate.Value.ToLocalTime();
            ev.EventEndDateTime = observationEvent.EndDate.Value.ToLocalTime();
            ev.EventStartDate = startDate;
            ev.EventEndDate = endDate;
            ev.EventStartTime = startTime;
            ev.EventEndTime = endTime;
            ev.SamplingProtocol = observationEvent.SamplingProtocol;
            ev.SurveyLocation = observationEvent?.Location?.ToLocation(responseCoordinateSystem);
            ev.EventType = observationEvent?.EventType;
            ev.LocationProtected = observationEvent.LocationProtected.GetValueOrDefault(false);
            ev.Weather = observationEvent?.Weather?.ToWeatherVariable();
            ev.RecorderCode = observationEvent.RecorderCode;
            ev.RecorderOrganisation = observationEvent?.RecorderOrganisation?.Select(m => m.ToOrganisation()).ToList();

            ev.OccurrenceIds = observationEvent?.OccurrenceIds;
            ev.NoObservations = ev.OccurrenceIds == null || !ev.OccurrenceIds.Any();

            return ev;
        }
    }

    private static void GetEventDates(
        DateTime startDate,
        DateTime endDate,
        string plainStartDate,
        string plainEndDate,
        string plainStartTime,
        string plainEndTime,
        out DateOnly startDateResult, 
        out DateOnly endDateResult, 
        out TimeOnly? startTimeResult, 
        out TimeOnly? endTimeResult)
    {
        if (!string.IsNullOrEmpty(plainStartDate) && DateOnly.TryParse(plainStartDate, out startDateResult))
        {                
        }
        else
        {
            var localStartDate = startDate.ToLocalTime();
            startDateResult = new DateOnly(localStartDate.Year, localStartDate.Month, localStartDate.Day);
        }

        if (!string.IsNullOrEmpty(plainEndDate) && DateOnly.TryParse(plainEndDate, out endDateResult))
        {                
        }
        else
        {
            var localEndDate = endDate.ToLocalTime();
            endDateResult = new DateOnly(localEndDate.Year, localEndDate.Month, localEndDate.Day);
        }

        if (string.IsNullOrEmpty(plainStartTime))
        {
            startTimeResult = null;
        }
        else 
        {
            if (TimeOnly.TryParse(plainStartTime, out var startTimeRes))
            {
                startTimeResult = startTimeRes;
            }
            else
            {
                startTimeResult = null;
            }
        }

        if (string.IsNullOrEmpty(plainEndTime))
        {
            endTimeResult = null;
        }
        else
        {
            if (TimeOnly.TryParse(plainEndTime, out var endTimeRes))
            {
                endTimeResult = endTimeRes;
            }
            else
            {
                endTimeResult = null;
            }
        }            
    }

    extension(ProcessedDataStewardship.Event.WeatherVariable source)
    {
        public WeatherVariable ToWeatherVariable()
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
    }

    extension(ProcessedDataStewardship.Enums.Cloudiness source)
    {
        private Cloudiness ToCloudiness()
        {
            return (Cloudiness)source;
        }
    }

    extension(ProcessedDataStewardship.Enums.Visibility source)
    {
        private Visibility ToVisibility()
        {
            return (Visibility)source;
        }
    }

    extension(ProcessedDataStewardship.Enums.Precipitation source)
    {
        private Precipitation ToPrecipitation()
        {
            return (Precipitation)source;
        }
    }

    extension(ProcessedDataStewardship.Enums.WindStrength source)
    {
        private WindStrength ToWindStrength()
        {
            return (WindStrength)source;
        }
    }

    extension(ProcessedDataStewardship.Enums.WindDirectionCompass source)
    {
        private WindDirectionCompass ToWindDirectionCompass()
        {
            return (WindDirectionCompass)source;
        }
    }

    extension(ProcessedDataStewardship.Enums.Unit source)
    {
        private Contracts.Enums.Unit ToUnit()
        {
            return (Contracts.Enums.Unit)source;
        }
    }

    extension(ProcessedDataStewardship.Enums.SnowCover source)
    {
        private SnowCover ToSnowCover()
        {
            return (SnowCover)source;
        }
    }

    extension(ProcessedDataStewardship.Event.WeatherMeasuring source)
    {
        public WeatherMeasuring ToWeatherMeasuring()
        {
            if (source == null) return null;

            return new WeatherMeasuring
            {
                Unit = source.Unit?.ToUnit(),
                WeatherMeasure = source.WeatherMeasure
            };
        }
    }

    extension(DataStewardshipInfo source)
    {
        public DatasetInfo ToDatasetInfo()
        {
            if (source == null) return null;
            return new DatasetInfo
            {
                Identifier = source.DatasetIdentifier,
                Title = source.DatasetTitle,
            };
        }
    }

    extension(Observation observation)
    {
        public Contracts.Models.Event ToEventModel(IEnumerable<string> occurrenceIds, CoordinateSystem responseCoordinateSystem)
        {
            if (observation == null) return null;

            GetEventDates(
                observation.Event.StartDate.Value,
                observation.Event.EndDate.Value,
                observation.Event.PlainStartDate,
                observation.Event.PlainEndDate,
                observation.Event.PlainStartTime,
                observation.Event.PlainEndTime,
                out DateOnly startDate,
                out DateOnly endDate,
                out TimeOnly? startTime,
                out TimeOnly? endTime);

            var ev = new Contracts.Models.Event();
            ev.EventID = observation.Event.EventId;
            ev.ParentEventID = observation.Event.ParentEventId;
            ev.EventRemarks = observation.Event.EventRemarks;
            ev.AssociatedMedia = observation.Event.Media.ToAssociatedMedias();
            ev.Dataset = new DatasetInfo
            {
                Identifier = observation.DataStewardship?.DatasetIdentifier,
                Title = observation.DataStewardship?.DatasetTitle
            };

            ev.EventStartDateTime = observation.Event.StartDate.Value;
            ev.EventEndDateTime = observation.Event.EndDate.Value;
            ev.EventStartDate = startDate;
            ev.EventEndDate = endDate;
            ev.EventStartTime = startTime;
            ev.EventEndTime = endTime;
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
                ev.RecorderOrganisation = new List<Contracts.Models.Organisation>
            {
                new Contracts.Models.Organisation
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

        public Contracts.Models.Occurrence ToOccurrenceModel(CoordinateSystem responseCoordinateSystem)
        {
            var occurrence = new Contracts.Models.Occurrence();
            occurrence.AssociatedMedia = observation.Occurrence?.Media.ToAssociatedMedias();
            if (observation?.BasisOfRecord?.Id != null)
            {
                occurrence.BasisOfRecord = GetBasisOfRecordEnum((BasisOfRecordId)observation.BasisOfRecord.Id);
            }

            occurrence.EventID = observation.Event.EventId;
            occurrence.Dataset = observation?.DataStewardship?.ToDatasetInfo();
            occurrence.IdentificationVerificationStatus = observation?.Identification?.VerificationStatus?.Value;
            occurrence.ObservationCertainty = observation?.Location?.CoordinateUncertaintyInMeters == null ? null : Convert.ToDouble(observation.Location.CoordinateUncertaintyInMeters);
            occurrence.ObservationPoint = observation?.Location?.Point.Transform(CoordinateSys.WGS84, responseCoordinateSystem.ToCoordinateSys());
            occurrence.EventStartDate = observation.Event.StartDate?.ToLocalTime();
            occurrence.EventEndDate = observation.Event.EndDate?.ToLocalTime();
            occurrence.ObservationTime = observation.Event.StartDate == observation.Event.EndDate ? observation.Event.StartDate?.ToLocalTime() : null;
            occurrence.OccurrenceID = observation.Occurrence.OccurrenceId;
            occurrence.OccurrenceRemarks = observation.Occurrence.OccurrenceRemarks;
            occurrence.OccurrenceStatus = observation.Occurrence.IsPositiveObservation ? OccurrenceStatus.Observerad : OccurrenceStatus.InteObserverad;
            occurrence.Quantity = observation.Occurrence.OrganismQuantityInt != null ? Convert.ToDouble(observation.Occurrence.OrganismQuantityInt) : 0;
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
    }

    extension(Lib.Models.Processed.Observation.Location location)
    {
        public Contracts.Models.Location ToLocation(CoordinateSystem responseCoordinateSystem)
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
                LocationType = GetLocationType(location),
                CoordinateUncertaintyInMeters = location?.CoordinateUncertaintyInMeters,
                Emplacement = location?.Point.Transform(CoordinateSys.WGS84, responseCoordinateSystem.ToCoordinateSys()), // todo - decide if to use Point or PointWithBuffer                
            };
        }
    }

    private static Contracts.Enums.LocationType GetLocationType(Lib.Models.Processed.Observation.Location location)
    {
        switch(location.Type)
        {
            case Lib.Enums.LocationType.Point:
                return Contracts.Enums.LocationType.Punkt;
            case Lib.Enums.LocationType.Polygon:
                return Contracts.Enums.LocationType.Polygon;
            default:
                return Contracts.Enums.LocationType.Punkt;
        }
    }

    extension(IEnumerable<Multimedia> multimedias)
    {
        public List<AssociatedMedia> ToAssociatedMedias()
        {
            if (multimedias == null || !multimedias.Any()) return null;
            return multimedias.Select(m => m.ToAssociatedMedia()).ToList();
        }
    }

    extension(Multimedia multimedia)
    {
        public AssociatedMedia ToAssociatedMedia()
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

    extension(Lib.Models.Processed.Observation.Taxon taxon)
    {
        public Contracts.Models.Taxon ToTaxonModel()
        {
            return new Contracts.Models.Taxon
            {
                ScientificName = taxon.ScientificName,
                TaxonID = taxon.Id.ToString(),
                TaxonRank = taxon.TaxonRank,
                VernacularName = taxon.VernacularName,
                VerbatimTaxonID = taxon.VerbatimId,
                VerbatimName = taxon.VerbatimName
            };
        }
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

    extension(DatasetFilter datasetFilter)
    {
        public SearchFilter ToSearchFilter()
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
    }

    extension(EventsFilter eventsFilter)
    {
        public SearchFilter ToSearchFilter()
        {
            if (eventsFilter == null) return null;

            var filter = new SearchFilter(0);
            filter.DataStewardshipDatasetIds = eventsFilter.DatasetIds ?? eventsFilter.DatasetList;
            filter.Event = new EventFilter
            {
                Ids = eventsFilter.EventIds
            };
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
    }

    extension(OccurrenceFilter occurrenceFilter)
    {
        public SearchFilter ToSearchFilter()
        {
            if (occurrenceFilter == null) return null;

            var filter = new SearchFilter(0);
            filter.Event = new EventFilter { Ids = occurrenceFilter.EventIds };
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
    }

    extension(Contracts.Models.GeographicsFilter geographicsFilter)
    {
        public LocationFilter ToLocationFilter()
        {
            if (geographicsFilter == null) return null;
            var locationFilter = new LocationFilter();
            var areaFilter = new List<AreaFilter>();

            // County
            if (geographicsFilter.County != null)
            {
                areaFilter.Add(new AreaFilter
                {
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
    }

    extension(GeometryFilter geographicsFilterArea)
    {
        public SOS.Lib.Models.Search.Filters.GeographicsFilter ToGeographicsFilter()
        {
            if (geographicsFilterArea == null) return null;
            var geographicsFilter = new SOS.Lib.Models.Search.Filters.GeographicsFilter();
            geographicsFilter.MaxDistanceFromPoint = geographicsFilterArea.MaxDistanceFromGeometries;
            if (geographicsFilterArea.GeographicArea != null)
            {
                geographicsFilter.Geometries = new() { geographicsFilterArea.GeographicArea }; // todo - change filter type to List<IGeoShape>?
            }

            return geographicsFilter;
        }
    }

    extension(DateFilterType dateFilterType)
    {
        public Lib.Models.Search.Filters.DateFilter.DateRangeFilterType ToDateRangeFilterType()
        {
            return dateFilterType switch
            {
                DateFilterType.OnlyStartDate => Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.OnlyStartDate,
                DateFilterType.OnlyEndDate => Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.OnlyEndDate,
                DateFilterType.OverlappingStartDateAndEndDate => Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate,
                DateFilterType.BetweenStartDateAndEndDate => Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.BetweenStartDateAndEndDate,
                _ => Lib.Models.Search.Filters.DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate
            };
        }
    }
}