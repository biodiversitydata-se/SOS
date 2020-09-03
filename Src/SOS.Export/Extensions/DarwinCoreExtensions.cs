using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SOS.Export.Models.DarwinCore;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Export.Extensions
{
    /// <summary>
    ///     Extensions for Darwin Core
    /// </summary>
    public static class DarwinCoreExtensions
    {
        /// <summary>
        ///     Cast processed Darwin Core object to Darwin Core Archive
        /// </summary>
        /// <param name="processedDarwinCore"></param>
        /// <returns></returns>
        public static DwC ToDarwinCoreArchive(this DarwinCore processedDarwinCore)
        {
            if (processedDarwinCore == null)
            {
                return null;
            }

            return new DwC
            {
                AccessRights = processedDarwinCore.AccessRights,
                BasisOfRecord = processedDarwinCore.BasisOfRecord,
                BibliographicCitation = processedDarwinCore.BibliographicCitation,
                CollectionCode = processedDarwinCore.CollectionCode,
                CollectionID = processedDarwinCore.CollectionID,
                DataGeneralizations = processedDarwinCore.DataGeneralizations,
                DatasetID = processedDarwinCore.DatasetID,
                DatasetName = processedDarwinCore.DatasetName,
                DynamicProperties = JsonConvert.SerializeObject(processedDarwinCore.DynamicProperties),
                InformationWithheld = processedDarwinCore.InformationWithheld,
                InstitutionCode = processedDarwinCore.InstitutionCode,
                InstitutionID = processedDarwinCore.InstitutionID,
                Language = processedDarwinCore.Language,
                Modified = processedDarwinCore.Modified,
                References = processedDarwinCore.References,
                Rights = processedDarwinCore.Rights,
                RightsHolder = processedDarwinCore.RightsHolder,
                Type = processedDarwinCore.Type
            };
        }

        /// <summary>
        ///     Cast processed Darwin Core objects to Darwin Core
        /// </summary>
        /// <param name="processedDarwinCore"></param>
        /// <returns></returns>
        public static IEnumerable<DwC> ToDarwinCoreArchive(
            this IEnumerable<DarwinCore> processedDarwinCore)
        {
            return processedDarwinCore?.Select(m => m.ToDarwinCoreArchive());
        }

        /// <summary>
        ///     Cast processed Darwin Core event object to Darwin Core Archive
        /// </summary>
        /// <param name="source"></param>
        /// <param name="coreId"></param>
        /// <returns></returns>
        public static DwCEvent ToDarwinCoreArchive(this DarwinCoreEvent source, string coreId)
        {
            if (source == null)
            {
                return null;
            }

            return new DwCEvent
            {
                CoreID = coreId,
                Day = source.Day,
                EndDayOfYear = source.EndDayOfYear,
                EventDate = source.EventDate,
                EventID = source.EventID,
                EventRemarks = source.EventRemarks,
                EventTime = source.EventTime,
                FieldNotes = source.FieldNotes,
                FieldNumber = source.FieldNumber,
                Habitat = source.Habitat,
                Month = source.Month,
                ParentEventID = source.ParentEventID,
                SampleSizeUnit = source.SampleSizeUnit,
                SampleSizeValue = source.SampleSizeValue,
                SamplingEffort = source.SamplingEffort,
                SamplingProtocol = source.SamplingProtocol,
                StartDayOfYear = source.StartDayOfYear,
                VerbatimEventDate = source.VerbatimEventDate,
                Year = source.Year
            };
        }

        /// <summary>
        ///     Cast processed Darwin Core geological context object to Darwin Core Archive
        /// </summary>
        /// <param name="source"></param>
        /// <param name="coreId"></param>
        /// <returns></returns>
        public static DwCGeologicalContext ToDarwinCoreArchive(this DarwinCoreGeologicalContext source, string coreId)
        {
            if (source == null)
            {
                return null;
            }

            return new DwCGeologicalContext
            {
                CoreID = coreId,
                Bed = source.Bed,
                EarliestAgeOrLowestStage = source.EarliestAgeOrLowestStage,
                EarliestEonOrLowestEonothem = source.EarliestEonOrLowestEonothem,
                EarliestEpochOrLowestSeries = source.EarliestEpochOrLowestSeries,
                EarliestEraOrLowestErathem = source.EarliestEraOrLowestErathem,
                EarliestPeriodOrLowestSystem = source.EarliestPeriodOrLowestSystem,
                Formation = source.Formation,
                GeologicalContextID = source.GeologicalContextID,
                Group = source.Group,
                HighestBiostratigraphicZone = source.HighestBiostratigraphicZone,
                LatestAgeOrHighestStage = source.LatestAgeOrHighestStage,
                LatestEonOrHighestEonothem = source.LatestEonOrHighestEonothem,
                LatestEpochOrHighestSeries = source.LatestEpochOrHighestSeries,
                LatestEraOrHighestErathem = source.LatestEraOrHighestErathem,
                LatestPeriodOrHighestSystem = source.LatestPeriodOrHighestSystem,
                LithostratigraphicTerms = source.LithostratigraphicTerms,
                LowestBiostratigraphicZone = source.LowestBiostratigraphicZone,
                Member = source.Member
            };
        }

        /// <summary>
        ///     Cast processed Darwin Core Identification object to Darwin Core Archive
        /// </summary>
        /// <param name="source"></param>
        /// <param name="coreId"></param>
        /// <returns></returns>
        public static DwCIdentification ToDarwinCoreArchive(this DarwinCoreIdentification source, string coreId)
        {
            if (source == null)
            {
                return null;
            }

            return new DwCIdentification
            {
                CoreID = coreId,
                DateIdentified = source.DateIdentified,
                IdentificationID = source.IdentificationID,
                IdentificationQualifier = source.IdentificationQualifier,
                IdentificationReferences = source.IdentificationReferences,
                IdentificationRemarks = source.IdentificationRemarks,
                IdentificationVerificationStatus = source.IdentificationVerificationStatus,
                IdentifiedBy = source.IdentifiedBy,
                TypeStatus = source.TypeStatus
            };
        }

        /// <summary>
        ///     Cast processed Darwin Core Location object to Darwin Core Archive
        /// </summary>
        /// <param name="source"></param>
        /// <param name="coreId"></param>
        /// <returns></returns>
        public static DwCLocation ToDarwinCoreArchive(this DarwinCoreLocation source, string coreId)
        {
            if (source == null)
            {
                return null;
            }

            return new DwCLocation
            {
                CoreID = coreId,
                County = source.County,
                Municipality = source.Municipality,
                CountryCode = source.CountryCode,
                Continent = source.Continent,
                CoordinatePrecision = source.CoordinatePrecision,
                CoordinateUncertaintyInMeters = source.CoordinateUncertaintyInMeters,
                Country = source.Country,
                DecimalLatitude = source.DecimalLatitude,
                DecimalLongitude = source.DecimalLongitude,
                FootprintSRS = source.FootprintSRS,
                FootprintSpatialFit = source.FootprintSpatialFit,
                FootprintWKT = source.FootprintWKT,
                GeodeticDatum = source.GeodeticDatum,
                GeoreferenceProtocol = source.GeoreferenceProtocol,
                GeoreferenceRemarks = source.GeoreferenceRemarks,
                GeoreferenceSources = source.GeoreferenceSources,
                GeoreferenceVerificationStatus = source.GeoreferenceVerificationStatus,
                GeoreferencedBy = source.GeoreferencedBy,
                GeoreferencedDate = source.GeoreferencedDate,
                HigherGeography = source.HigherGeography,
                HigherGeographyID = source.HigherGeographyID,
                Island = source.Island,
                IslandGroup = source.IslandGroup,
                Locality = source.Locality,
                LocationAccordingTo = source.LocationAccordingTo,
                LocationID = source.LocationID,
                LocationRemarks = source.LocationRemarks,
                MaximumDepthInMeters = source.MaximumDepthInMeters,
                MaximumDistanceAboveSurfaceInMeters = source.MaximumDistanceAboveSurfaceInMeters,
                MaximumElevationInMeters = source.MaximumElevationInMeters,
                MinimumDepthInMeters = source.MinimumDepthInMeters,
                MinimumDistanceAboveSurfaceInMeters = source.MinimumDistanceAboveSurfaceInMeters,
                MinimumElevationInMeters = source.MinimumElevationInMeters,
                PointRadiusSpatialFit = source.PointRadiusSpatialFit,
                StateProvince = source.StateProvince,
                VerbatimCoordinateSystem = source.VerbatimCoordinateSystem,
                VerbatimCoordinates = source.VerbatimCoordinates,
                VerbatimDepth = source.VerbatimDepth,
                VerbatimElevation = source.VerbatimElevation,
                VerbatimLatitude = source.VerbatimLatitude,
                VerbatimLocality = source.VerbatimLocality,
                VerbatimLongitude = source.VerbatimLongitude,
                VerbatimSRS = source.VerbatimSRS,
                WaterBody = source.WaterBody
            };
        }

        /// <summary>
        ///     Cast processed Darwin Core Material Sample object to Darwin Core Archive
        /// </summary>
        /// <param name="source"></param>
        /// <param name="coreId"></param>
        /// <returns></returns>
        public static DwCMaterialSample ToDarwinCoreArchive(this DarwinCoreMaterialSample source, string coreId)
        {
            if (source == null)
            {
                return null;
            }

            return new DwCMaterialSample
            {
                CoreID = coreId,
                MaterialSampleID = source.MaterialSampleID
            };
        }

        /// <summary>
        ///     Cast processed Darwin Core Measurement Or Fact object to Darwin Core Archive
        /// </summary>
        /// <param name="source"></param>
        /// <param name="coreId"></param>
        /// <returns></returns>
        public static DwCMeasurementOrFact ToDarwinCoreArchive(this DarwinCoreMeasurementOrFact source, string coreId)
        {
            if (source == null)
            {
                return null;
            }

            return new DwCMeasurementOrFact
            {
                CoreID = coreId,
                MeasurementAccuracy = source.MeasurementAccuracy,
                MeasurementDeterminedBy = source.MeasurementDeterminedBy,
                MeasurementDeterminedDate = source.MeasurementDeterminedDate,
                MeasurementID = source.MeasurementID,
                MeasurementMethod = source.MeasurementMethod,
                MeasurementRemarks = source.MeasurementRemarks,
                MeasurementType = source.MeasurementType,
                MeasurementUnit = source.MeasurementUnit,
                MeasurementValue = source.MeasurementValue
            };
        }

        /// <summary>
        ///     Cast processed Darwin Core Occurrence object to Darwin Core Archive
        /// </summary>
        /// <param name="source"></param>
        /// <param name="coreId"></param>
        /// <returns></returns>
        public static DwCOccurrence ToDarwinCoreArchive(this DarwinCoreOccurrence source, string coreId)
        {
            if (source == null)
            {
                return null;
            }

            return new DwCOccurrence
            {
                CoreID = coreId,
                AssociatedMedia = source.AssociatedMedia,
                AssociatedOccurrences = source.AssociatedOccurrences,
                AssociatedReferences = source.AssociatedReferences,
                AssociatedSequences = source.AssociatedSequences,
                AssociatedTaxa = source.AssociatedTaxa,
                Behavior = source.Behavior,
                CatalogNumber = source.CatalogNumber,
                Disposition = source.Disposition,
                EstablishmentMeans = source.EstablishmentMeans,
                IndividualCount = source.IndividualCount,
                IndividualID = source.IndividualID,
                LifeStage = source.LifeStage,
                OccurrenceID = source.OccurrenceID,
                OccurrenceRemarks = source.OccurrenceRemarks,
                OccurrenceStatus = source.OccurrenceStatus,
                OrganismQuantity = source.OrganismQuantity,
                OrganismQuantityType = source.OrganismQuantityType,
                OtherCatalogNumbers = source.OtherCatalogNumbers,
                Preparations = source.Preparations,
                PreviousIdentifications = source.PreviousIdentifications,
                RecordNumber = source.RecordNumber,
                RecordedBy = source.RecordedBy,
                ReproductiveCondition = source.ReproductiveCondition,
                Sex = source.Sex
            };
        }

        /// <summary>
        ///     Cast processed Darwin Core Resource Relationship. object to Darwin Core Archive
        /// </summary>
        /// <param name="source"></param>
        /// <param name="coreId"></param>
        /// <returns></returns>
        public static DwCResourceRelationship ToDarwinCoreArchive(this DarwinCoreResourceRelationship source,
            string coreId)
        {
            if (source == null)
            {
                return null;
            }

            return new DwCResourceRelationship
            {
                CoreID = coreId,
                RelatedResourceID = source.RelatedResourceID,
                RelationshipAccordingTo = source.RelationshipAccordingTo,
                RelationshipEstablishedDate = source.RelationshipEstablishedDate,
                RelationshipOfResource = source.RelationshipOfResource,
                RelationshipRemarks = source.RelationshipRemarks,
                ResourceID = source.ResourceID,
                ResourceRelationshipID = source.ResourceRelationshipID
            };
        }

        /// <summary>
        ///     Cast processed Darwin Core taxon object to Darwin Core Archive
        /// </summary>
        /// <param name="source"></param>
        /// <param name="coreId"></param>
        /// <returns></returns>
        public static DwCTaxon ToDarwinCoreArchive(this DarwinCoreTaxon source, string coreId)
        {
            if (source == null)
            {
                return null;
            }

            return new DwCTaxon
            {
                CoreID = coreId,
                ScientificName = source.ScientificName,
                TaxonID = source.TaxonID,
                ParentNameUsageID = source.ParentNameUsageID,
                VernacularName = source.VernacularName,
                Kingdom = source.Kingdom,
                TaxonRank = source.TaxonRank,
                Family = source.Family,
                ScientificNameAuthorship = source.ScientificNameAuthorship,
                Order = source.Order,
                NomenclaturalStatus = source.NomenclaturalStatus,
                TaxonRemarks = source.TaxonRemarks,
                AcceptedNameUsageID = source.AcceptedNameUsageID,
                Genus = source.Genus,
                Phylum = source.Phylum,
                Class = source.Class,
                TaxonomicStatus = source.TaxonomicStatus,
                AcceptedNameUsage = source.AcceptedNameUsage,
                HigherClassification = source.HigherClassification,
                InfraspecificEpithet = source.InfraspecificEpithet,
                NameAccordingTo = source.NameAccordingTo,
                NameAccordingToID = source.NameAccordingToID,
                NamePublishedIn = source.NamePublishedIn,
                NamePublishedInID = source.NamePublishedInID,
                NamePublishedInYear = source.NamePublishedInYear,
                NomenclaturalCode = source.NomenclaturalCode,
                OriginalNameUsage = source.OriginalNameUsage,
                OriginalNameUsageID = source.OriginalNameUsageID,
                ParentNameUsage = source.ParentNameUsage,
                ScientificNameID = source.ScientificNameID,
                SpecificEpithet = source.SpecificEpithet,
                Subgenus = source.Subgenus,
                TaxonConceptID = source.TaxonConceptID,
                VerbatimTaxonRank = source.VerbatimTaxonRank
            };
        }

        public static IEnumerable<ExtendedMeasurementOrFactRow> ToExtendedMeasurementOrFactRows(this
            IEnumerable<ProcessedObservation> processedObservations)
        {
            return processedObservations.SelectMany(ToExtendedMeasurementOrFactRows);
        }

        public static IEnumerable<ExtendedMeasurementOrFactRow> ToExtendedMeasurementOrFactRows(this
            ProcessedObservation observation)
        {
            IEnumerable<ExtendedMeasurementOrFactRow> occurrenceEmof = null;
            IEnumerable<ExtendedMeasurementOrFactRow> eventEmof = null;
            IEnumerable<ExtendedMeasurementOrFactRow> projectEmof = null;
            if (observation.MeasurementOrFacts != null)
            {
                occurrenceEmof = observation.MeasurementOrFacts.Select(m => m.ToExtendedMeasurementOrFactRow());
            }

            if (observation.Event.MeasurementOrFacts != null)
            {
                eventEmof = observation.Event.MeasurementOrFacts.Select(m => m.ToExtendedMeasurementOrFactRow(observation.Event.EventId));
            }

            if (observation.Projects != null)
            {
                projectEmof = observation.Projects.SelectMany(project =>
                    ToExtendedMeasurementOrFactRows(project, observation.Occurrence.OccurrenceId));
            }

            return (occurrenceEmof ?? Enumerable.Empty<ExtendedMeasurementOrFactRow>())
                .Union(eventEmof ?? Enumerable.Empty<ExtendedMeasurementOrFactRow>())
                .Union(projectEmof ?? Enumerable.Empty<ExtendedMeasurementOrFactRow>());
        }

        private static ExtendedMeasurementOrFactRow ToExtendedMeasurementOrFactRow(
            this ProcessedExtendedMeasurementOrFact processedEmof, string eventId = null)
        {
            return new ExtendedMeasurementOrFactRow
            {
                MeasurementAccuracy = processedEmof.MeasurementAccuracy,
                MeasurementDeterminedBy = processedEmof.MeasurementDeterminedBy,
                MeasurementDeterminedDate = processedEmof.MeasurementDeterminedDate,
                MeasurementID = processedEmof.MeasurementID,
                MeasurementMethod = processedEmof.MeasurementMethod,
                MeasurementRemarks = !string.IsNullOrEmpty(eventId) ? ($"Measurement for EventID \"{eventId}\". " + processedEmof.MeasurementRemarks).TrimEnd() : processedEmof.MeasurementRemarks,
                MeasurementType = processedEmof.MeasurementType,
                MeasurementTypeID = processedEmof.MeasurementTypeID,
                MeasurementUnit = processedEmof.MeasurementUnit,
                MeasurementUnitID = processedEmof.MeasurementUnitID,
                MeasurementValue = processedEmof.MeasurementValue,
                MeasurementValueID = processedEmof.MeasurementValueID,
                OccurrenceID = processedEmof.OccurrenceID
            };
        }

        public static IEnumerable<ExtendedMeasurementOrFactRow> ToExtendedMeasurementOrFactRows(this
            IEnumerable<ProcessedProject> projects, string occurrenceId)
        {
            return projects.SelectMany(project => ToExtendedMeasurementOrFactRows(project, occurrenceId));
        }

        private static IEnumerable<ExtendedMeasurementOrFactRow> ToExtendedMeasurementOrFactRows(
            ProcessedProject project, string occurrenceId)
        {
            if (project?.ProjectParameters == null || !project.ProjectParameters.Any())
            {
                return Enumerable.Empty<ExtendedMeasurementOrFactRow>();
            }

            var rows = project.ProjectParameters.Select(projectParameter =>
                ToExtendedMeasurementOrFactRow(occurrenceId, project, projectParameter));
            return rows;
        }

        private static ExtendedMeasurementOrFactRow ToExtendedMeasurementOrFactRow(
            string occurrenceId,
            ProcessedProject project,
            ProcessedProjectParameter projectParameter)
        {
            var row = new ExtendedMeasurementOrFactRow();
            row.OccurrenceID = occurrenceId;
            row.MeasurementID = project.Id; // Should this be ProjectId or ProjectParameterId?
            //row.MeasurementID = projectParameter.ProjectParameterId.ToString(); // Should this be ProjectId or ProjectParameterId?
            row.MeasurementType = projectParameter.Name;
            row.MeasurementValue = projectParameter.Value;
            row.MeasurementUnit = projectParameter.Unit;
            row.MeasurementDeterminedDate = DwcFormatter.CreateDateIntervalString(project.StartDate, project.EndDate);
            row.MeasurementMethod = GetMeasurementMethodDescription(project);
            row.MeasurementRemarks = projectParameter.Description;

            //row.MeasurementAccuracy = ?
            //row.MeasurementDeterminedBy = ?
            //row.MeasurementRemarks = ?
            //row.MeasurementTypeID = ?
            //row.MeasurementUnitID = ?
            //row.MeasurementValueID = ?
            return row;
        }

        private static string GetMeasurementMethodDescription(ProcessedProject project)
        {
            if (string.IsNullOrEmpty(project.SurveyMethod) && string.IsNullOrEmpty(project.SurveyMethodUrl))
            {
                return null;
            }

            if (string.IsNullOrEmpty(project.SurveyMethodUrl))
            {
                return project.SurveyMethod;
            }

            if (string.IsNullOrEmpty(project.SurveyMethod))
            {
                return project.SurveyMethodUrl;
            }

            return $"{project.SurveyMethod} [{project.SurveyMethodUrl}]";
        }
    }
}