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
        private static DsActivity? GetActivityEnum(ActivityId? activityId)
        {
            if (activityId == null) return null;

            return activityId.Value switch
            {
                ActivityId.AgitatedBehaviour => DsActivity.UpprördVarnande,
                ActivityId.BreedingFailed => DsActivity.MisslyckadHäckning,
                ActivityId.BuildingNestOrUsedNestOrNest => DsActivity.Bobygge,
                ActivityId.Call => DsActivity.LockläteÖvrigaLäten,
                ActivityId.CarryingFoodForYoung => DsActivity.FödaÅtUngar,
                ActivityId.DeadCollidedWithAeroplane => DsActivity.Död,
                ActivityId.DeadCollidedWithFence => DsActivity.Död,
                ActivityId.DeadCollidedWithLighthouse => DsActivity.Död,
                ActivityId.DeadCollidedWithPowerLine => DsActivity.Död,
                ActivityId.DeadCollidedWithWindMill => DsActivity.Död,
                ActivityId.DeadCollidedWithWindow => DsActivity.Död,
                ActivityId.DeadDueToDiseaseOrStarvation => DsActivity.Död,
                ActivityId.Display => DsActivity.SpelSång,
                ActivityId.DisplayOrSong => DsActivity.SpelSång,
                ActivityId.DisplayOrSongOutsideBreeding => DsActivity.SpelSångEjHäckning,
                ActivityId.DisputeBetweenMales => DsActivity.Revirhävdande,
                ActivityId.DistractionDisplay => DsActivity.AvledningsbeteendeEnum,
                ActivityId.Dormant => DsActivity.Vilande,
                ActivityId.EggLaying => DsActivity.Äggläggande,
                ActivityId.EggShells => DsActivity.Äggskal,
                ActivityId.FlyingOverhead => DsActivity.Förbiflygande,
                ActivityId.Foraging => DsActivity.Födosökande,
                ActivityId.FoundDead => DsActivity.Död,
                ActivityId.Fragment => DsActivity.Fragment,
                ActivityId.Freeflying => DsActivity.Friflygande,
                ActivityId.FreshGnaw => DsActivity.FärskaGnagspår,
                ActivityId.Gnaw => DsActivity.ÄldreGnagspår,
                ActivityId.Incubating => DsActivity.Ruvande,
                ActivityId.InNestingHabitat => DsActivity.ObsIHäcktidLämpligBiotop,
                ActivityId.InWater => DsActivity.IVattenSimmande,
                ActivityId.InWaterOrSwimming => DsActivity.IVattenSimmande,
                ActivityId.KilledByElectricity => DsActivity.Död,
                ActivityId.KilledByOil => DsActivity.Död,
                ActivityId.KilledByPredator => DsActivity.Död,
                ActivityId.MatingOrMatingCeremonies => DsActivity.Parning,
                ActivityId.Migrating => DsActivity.Sträckande,
                ActivityId.MigratingE => DsActivity.Sträckande,
                ActivityId.MigratingFish => DsActivity.Sträckande,
                ActivityId.MigratingN => DsActivity.Sträckande,
                ActivityId.MigratingNE => DsActivity.Sträckande,
                ActivityId.MigratingNW => DsActivity.Sträckande,
                ActivityId.MigratingS => DsActivity.Sträckande,
                ActivityId.MigratingSE => DsActivity.Sträckande,
                ActivityId.MigratingSW => DsActivity.Sträckande,
                ActivityId.MigratingW => DsActivity.Sträckande,
                ActivityId.NestBuilding => DsActivity.Bobygge,
                ActivityId.NestWithChickHeard => DsActivity.BoHördaUngar,
                ActivityId.NestWithEgg => DsActivity.BoÄggUngar,
                ActivityId.OldNest => DsActivity.AnväntBo,
                ActivityId.PairInSuitableHabitat => DsActivity.ParILämpligHäckbiotop,
                ActivityId.PermanentTerritory => DsActivity.PermanentRevir,
                ActivityId.PregnantFemale => DsActivity.DräktigHona,
                ActivityId.RecentlyFledgedYoung => DsActivity.PulliNyligenFlyggaUngar,
                ActivityId.RecentlyUsedNest => DsActivity.AnväntBo,
                ActivityId.RoadKill => DsActivity.Död,
                ActivityId.RunningOrCrawling => DsActivity.FrispringandeKrypande,
                ActivityId.ShotOrKilled => DsActivity.Död,
                ActivityId.SignsOfGnawing => DsActivity.FärskaGnagspår,
                ActivityId.Staging => DsActivity.Rastande,
                ActivityId.Stationary => DsActivity.Stationär,
                ActivityId.Territorial => DsActivity.Revirhävdande,
                ActivityId.TerritoryOutsideBreeding => DsActivity.Revirhävdande,
                ActivityId.VisitingOccupiedNest => DsActivity.BesökerBebottBo,
                ActivityId.VisitPossibleNest => DsActivity.Bobesök,
                ActivityId.WinterHabitat => DsActivity.PåÖvervintringsplats,
                _ => null
            };
        }

        private static DsBasisOfRecord GetBasisOfRecordEnum(BasisOfRecordId? basisOfRecordId)
        {
            return basisOfRecordId switch
            {
                BasisOfRecordId.HumanObservation => DsBasisOfRecord.MänskligObservation,
                BasisOfRecordId.MachineObservation => DsBasisOfRecord.MaskinellObservation,
                BasisOfRecordId.MaterialSample => DsBasisOfRecord.FysisktProv,
                _ => DsBasisOfRecord.Okänt
            };
        }

        private static DsLifeStage? GetLifeStageEnum(LifeStageId? lifeStageId)
        {
            if (lifeStageId == null) return null;

            return lifeStageId switch
            {
                LifeStageId.Adult => DsLifeStage.Adult,
                LifeStageId.AtLeast1StCalendarYear => DsLifeStage._1KPlus,
                LifeStageId.AtLeast2NdCalendarYear => DsLifeStage._2KPlus,
                LifeStageId.AtLeast3RdCalendarYear => DsLifeStage._3KPlus,
                LifeStageId.AtLeast4ThCalendarYear => DsLifeStage._4KPlus,
                LifeStageId.BudBurst => DsLifeStage.Knoppbristning,
                LifeStageId.Cub => DsLifeStage.Årsunge,
                LifeStageId.Egg => DsLifeStage.Ägg,
                LifeStageId.FirstCalendarYear => DsLifeStage._1K,
                LifeStageId.Flowering => DsLifeStage.Blomning,
                LifeStageId.FourthCalendarYear => DsLifeStage._4K,
                LifeStageId.FourthCalendarYearOrYounger => DsLifeStage._4KMinus,
                LifeStageId.FullyDevelopedLeaf => DsLifeStage.FulltUtveckladeBlad,
                LifeStageId.ImagoOrAdult => DsLifeStage.ImagoAdult,
                LifeStageId.Juvenile => DsLifeStage.Juvenil,
                LifeStageId.Larvae => DsLifeStage.Larv,
                LifeStageId.LarvaOrNymph => DsLifeStage.LarvNymf,
                LifeStageId.LeafCutting => DsLifeStage.GulnadeLövBlad,
                LifeStageId.Nestling => DsLifeStage.Pulli,
                LifeStageId.OnePlus => DsLifeStage._1KPlus,
                LifeStageId.Overblown => DsLifeStage.Överblommad,
                LifeStageId.Pupa => DsLifeStage.Puppa,
                LifeStageId.Rest => DsLifeStage.Vilstadium,
                LifeStageId.SecondCalendarYear => DsLifeStage._2K,
                LifeStageId.Sprout => DsLifeStage.MedGroddkorn,
                LifeStageId.ThirdCalendarYear => DsLifeStage._3K,
                LifeStageId.ThirdCalendarYearOrYounger => DsLifeStage._3KMinus,
                LifeStageId.WithCapsule => DsLifeStage.MedKapsel,
                LifeStageId.WithFemaleParts => DsLifeStage.MedHonorgan,
                LifeStageId.WithoutCapsule => DsLifeStage.UtanKapsel,
                LifeStageId.YellowingLeaves => DsLifeStage.GulnadeLövBlad,
                LifeStageId.ZeroPlus => DsLifeStage.Årsyngel,
                _ => null
            };
        }

        private static DsQuantityVariable? GetQuantityVariableEnum(UnitId unitId)
        {
            return unitId switch
            {
                UnitId.CoverClass => DsQuantityVariable.Yttäckning,
                UnitId.CoverPercentage => DsQuantityVariable.Täckningsgrad,
                UnitId.Individuals => DsQuantityVariable.AntalIndivider,
                UnitId.Fruitbodies => DsQuantityVariable.AntalFruktkroppar,
                UnitId.Capsules => DsQuantityVariable.AntalKapslar,
                UnitId.Plants => DsQuantityVariable.AntalPlantorTuvor,
                UnitId.Stems => DsQuantityVariable.AntalStjälkarStrånSkott,
                UnitId.EggClusters => DsQuantityVariable.AntalÄggklumpar,
                _ => null
            };
        }

        private static DsSex? GetSexEnum(SexId? sexId)
        {
            if (sexId == null) return null;

            return sexId.Value switch
            {
                SexId.Male => DsSex.Hane,
                SexId.Female => DsSex.Hona,
                SexId.InPair => DsSex.IPar,
                _ => null!
            };
        }

        /// <summary>
        /// Cast event model to csv
        /// </summary>
        /// <param name="occurrence"></param>
        /// <returns></returns>
        public static byte[] ToCsv(this DsOccurrenceDto occurrence)
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
        public static byte[] ToCsv(this IEnumerable<DsOccurrenceDto> occurrences)
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

        public static DsOccurrenceDto ToDto(this Lib.Models.Processed.Observation.Observation observation, CoordinateSys responseCoordinateSystem)
        {
            var occurrence = new DsOccurrenceDto();
            occurrence.AssociatedMedia = observation.Occurrence?.Media.ToDtos();
            if (observation?.BasisOfRecord?.Id != null)
            {
                occurrence.BasisOfRecord = GetBasisOfRecordEnum((BasisOfRecordId)observation.BasisOfRecord.Id);
            }

            occurrence.EventID = observation.Event.EventId;
            occurrence.Dataset ??= new DsDatasetInfoDto();
            occurrence.Dataset.Identifier = observation?.DataStewardship?.DatasetIdentifier;
            occurrence.IdentificationVerificationStatus = observation?.Identification?.VerificationStatus?.Value;
            occurrence.ObservationCertainty = observation?.Location?.CoordinateUncertaintyInMeters == null ? null : Convert.ToDouble(observation.Location.CoordinateUncertaintyInMeters);
            occurrence.ObservationPoint = observation?.Location?.Point.ConvertCoordinateSystem(responseCoordinateSystem);
            occurrence.EventStartDate = observation.Event.StartDate;
            occurrence.EventEndDate = observation.Event.EndDate;
            occurrence.ObservationTime = observation.Event.StartDate == observation.Event.EndDate ? observation.Event.StartDate : null;
            occurrence.OccurrenceID = observation.Occurrence.OccurrenceId;
            occurrence.OccurrenceRemarks = observation.Occurrence.OccurrenceRemarks;
            occurrence.OccurrenceStatus = observation.Occurrence.IsPositiveObservation ? DsOccurrenceStatus.Observerad : DsOccurrenceStatus.InteObserverad;
            occurrence.Quantity = Convert.ToDouble(observation.Occurrence.OrganismQuantityInt);
            if (observation?.Occurrence?.OrganismQuantityUnit?.Id != null)
            {
                occurrence.QuantityVariable = GetQuantityVariableEnum((UnitId)observation.Occurrence.OrganismQuantityUnit.Id);
            }
            occurrence.Taxon = observation?.Taxon?.ToDto();

            //occurrence.Unit = ?
            occurrence.Organism = new DsOrganismVariableDto
            {
                Sex = observation?.Occurrence?.Sex?.Id == null ? null : GetSexEnum((SexId)observation.Occurrence.Sex.Id),
                Activity = observation?.Occurrence?.Activity?.Id == null ? null : GetActivityEnum((ActivityId)observation.Occurrence.Activity.Id),
                LifeStage = observation?.Occurrence?.LifeStage?.Id == null ? null : GetLifeStageEnum((LifeStageId)observation.Occurrence.LifeStage.Id),
            };

            return occurrence;
        }
    }
}
