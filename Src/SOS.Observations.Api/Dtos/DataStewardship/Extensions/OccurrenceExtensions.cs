using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Helpers;
using SOS.Observations.Api.Dtos.DataStewardship.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class OccurrenceExtensions
    {
        private static Activity? GetActivityEnum(ActivityId? activityId)
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

        private static BasisOfRecord GetBasisOfRecordEnum(BasisOfRecordId? basisOfRecordId)
        {
            return basisOfRecordId switch
            {
                BasisOfRecordId.HumanObservation => BasisOfRecord.MänskligObservation,
                BasisOfRecordId.MachineObservation => BasisOfRecord.MaskinellObservation,
                BasisOfRecordId.MaterialSample => BasisOfRecord.FysisktProv,
                _ => BasisOfRecord.Okänt
            };
        }

        private static LifeStage? GetLifeStageEnum(LifeStageId? lifeStageId)
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

        private static QuantityVariable? GetQuantityVariableEnum(UnitId unitId)
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

        private static Sex? GetSexEnum(SexId? sexId)
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

        /// <summary>
        /// Cast event model to csv
        /// </summary>
        /// <param name="occurrence"></param>
        /// <returns></returns>
        public static byte[] ToCsv(this OccurrenceDto occurrence)
        {
            if (occurrence == null)
            {
                return null!;
            }

            return new[] { occurrence }.ToCsv();
        }

        /// <summary>
        /// Caste event models to csv
        /// </summary>
        /// <param name="occurrences"></param>
        /// <returns></returns>
        public static byte[] ToCsv(this IEnumerable<OccurrenceDto> occurrences)
        {
            if (!occurrences?.Any() ?? true)
            {
                return null!;
            }

            using var stream = new MemoryStream();
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeWrite(stream, "\t");
            csvFileHelper.WriteRow(new[] {
                "occurrence id",
                "basis of record",
                "observation time",
                "start date",
                "end date",
                "lon/lat",
                "taxon",
                "status",
                "quantity/variable",
                "quantity",
                "unit",
                "organism",
                "remarks",
                "observationCertainty",
                "identification verification status",
                "associated media",
                "event id",
                "dataset identifier"
            });

            foreach (var occurrence in occurrences)
            {
                var point = (PointGeoShape)occurrence.ObservationPoint;
                var lon = point?.Coordinates.Longitude;
                var lat = point?.Coordinates.Latitude;

                csvFileHelper.WriteRow(new[] {
                    occurrence.OccurrenceID,
                    occurrence.BasisOfRecord?.ToString(),
                    occurrence.ObservationTime?.ToLongDateString(),
                    occurrence.EventStartDate?.ToLongDateString(),
                    occurrence.EventEndDate?.ToLongDateString(),
                    $"{lon}/{lat}",
                    $"{occurrence.Taxon?.TaxonID}/{occurrence.Taxon?.ScientificName}/{occurrence.Taxon?.VernacularName}",
                   occurrence.OccurrenceStatus?.ToString(),
                   occurrence.QuantityVariable?.ToString(),
                   occurrence.Quantity?.ToString(),
                   occurrence.Unit?.ToString(),
                   $"{occurrence.Organism?.Activity?.ToString()}/{occurrence.Organism?.LifeStage?.ToString()}/{occurrence.Organism?.Sex?.ToString()}",
                   occurrence.OccurrenceRemarks,
                   occurrence.ObservationCertainty?.ToString(),
                   occurrence.IdentificationVerificationStatus?.ToString(),
                   occurrence.AssociatedMedia?.Select(m => $"{m.AssociatedMediaName}/{m.AssociatedMediaType}/{m.AssociatedMediaLink}").Concat(),
                   occurrence.EventID,
                   occurrence.Dataset?.Identifier
                });
            }

            csvFileHelper.Flush();
            stream.Position = 0;
            var csv = stream.ToArray();
            csvFileHelper.FinishWrite();

            return csv;
        }

        public static OccurrenceDto ToDto(this Lib.Models.Processed.Observation.Observation observation, CoordinateSys responseCoordinateSystem)
        {
            var occurrence = new OccurrenceDto();
            occurrence.AssociatedMedia = observation.Occurrence?.Media.ToDtos();
            if (observation?.BasisOfRecord?.Id != null)
            {
                occurrence.BasisOfRecord = GetBasisOfRecordEnum((BasisOfRecordId)observation.BasisOfRecord.Id);
            }

            occurrence.EventID = observation.Event.EventId;
            occurrence.Dataset ??= new DatasetInfoDto();
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
            occurrence.Taxon = observation?.Taxon?.ToDto();

            //occurrence.Unit = ?
            occurrence.Organism = new OrganismVariableDto
            {
                Sex = observation?.Occurrence?.Sex?.Id == null ? null : GetSexEnum((SexId)observation.Occurrence.Sex.Id),
                Activity = observation?.Occurrence?.Activity?.Id == null ? null : GetActivityEnum((ActivityId)observation.Occurrence.Activity.Id),
                LifeStage = observation?.Occurrence?.LifeStage?.Id == null ? null : GetLifeStageEnum((LifeStageId)observation.Occurrence.LifeStage.Id),
            };

            return occurrence;
        }
    }
}
