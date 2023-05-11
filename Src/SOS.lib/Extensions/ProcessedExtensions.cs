using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;
using static SOS.Lib.Models.Processed.DataStewardship.Dataset.Dataset;
using static SOS.Lib.Models.Processed.DataStewardship.Event.Event;

namespace SOS.Lib.Extensions
{
    /// <summary>
    ///     Extensions for Darwin Core
    /// </summary>
    public static class ProcessedExtensions
    {
        #region Event

        public static DarwinCoreEvent ToDarwinCore(this Event source)
        {
            if (source == null)
            {
                return null;
            }

            return new DarwinCoreEvent
            {
                EventDate = DwcFormatter.CreateDateIntervalString(source.StartDate, source.EndDate),
                EventTime = DwcFormatter.CreateTimeIntervalString(source.StartDate, source.EndDate),
                Habitat = source.Habitat,
                SamplingProtocol = source.SamplingProtocol,
                VerbatimEventDate = source.VerbatimEventDate,
                Day = source.StartDate?.Day,
                Month = source.StartDate?.Month,
                Year = source.StartDate?.Year,
                StartDayOfYear = source.StartDate?.DayOfYear,
                EndDayOfYear = source.EndDate?.DayOfYear,
                EventID = source.EventId,
                EventRemarks = source.EventRemarks,
                FieldNotes = source.FieldNotes,
                FieldNumber = source.FieldNumber,
                ParentEventID = source.ParentEventId,
                SampleSizeUnit = source.SampleSizeUnit,
                SampleSizeValue = source.SampleSizeValue,
                SamplingEffort = source.SamplingEffort
            };
        }

        #endregion Event

        #region Identification

        public static DarwinCoreIdentification ToDarwinCore(this Identification source)
        {
            if (source == null)
            {
                return null;
            }

            string dateIdentifiedString = null;
            if (DateTime.TryParse(source.DateIdentified, out var dateIdentified))
            {
                dateIdentifiedString = $"{dateIdentified.ToString("s")}Z";
            }

            return new DarwinCoreIdentification
            {
                IdentificationVerificationStatus = source.VerificationStatus?.Value,
                IdentifiedBy = source.IdentifiedBy,
                IdentificationReferences = source.IdentificationReferences,
                IdentificationRemarks = source.IdentificationRemarks,
                IdentificationID = source.IdentificationId,
                IdentificationQualifier = source.IdentificationQualifier,
                DateIdentified = dateIdentifiedString,
                TypeStatus = source.TypeStatus
            };
        }

        #endregion Identification

        #region Location

        public static DarwinCoreLocation ToDarwinCore(this Location source)
        {
            if (source == null)
            {
                return null;
            }

            return new DarwinCoreLocation
            {
                Continent = source.Continent?.Value,
                CoordinatePrecision = source.CoordinatePrecision?.ToString(),
                CoordinateUncertaintyInMeters = source.CoordinateUncertaintyInMeters,
                Country = source.Country?.Value,
                CountryCode = source.CountryCode,
                County = source.County?.Name,
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
                HigherGeographyID = source.HigherGeographyId,
                Island = source.Island,
                IslandGroup = source.IslandGroup,
                Locality = source.Locality,
                LocationAccordingTo = source.LocationAccordingTo,
                LocationID = source.LocationId,
                LocationRemarks = source.LocationRemarks,
                MaximumDepthInMeters = source.MaximumDepthInMeters?.ToString(),
                MaximumDistanceAboveSurfaceInMeters = source.MaximumDistanceAboveSurfaceInMeters?.ToString(),
                MaximumElevationInMeters = source.MaximumElevationInMeters?.ToString(),
                MinimumDepthInMeters = source.MinimumDepthInMeters?.ToString(),
                MinimumDistanceAboveSurfaceInMeters = source.MinimumDistanceAboveSurfaceInMeters?.ToString(),
                MinimumElevationInMeters = source.MinimumElevationInMeters?.ToString(),
                Municipality = source.Municipality?.Name,
                PointRadiusSpatialFit = source.PointRadiusSpatialFit,
                StateProvince = source.Province?.Name,
                VerbatimCoordinates = null,
                VerbatimCoordinateSystem = source.VerbatimCoordinateSystem,
                VerbatimDepth = source.VerbatimDepth,
                VerbatimElevation = source.VerbatimElevation,
                VerbatimLatitude = source.VerbatimLatitude,
                VerbatimLocality = source.VerbatimLocality,
                VerbatimLongitude = source.VerbatimLongitude,
                VerbatimSRS = source.VerbatimSRS,
                WaterBody = source.WaterBody
            };
        }

        #endregion Location

        #region Occurrence

        public static DarwinCoreOccurrence ToDarwinCore(this Occurrence source)
        {
            if (source == null)
            {
                return null;
            }

            return new DarwinCoreOccurrence
            {
                AssociatedMedia = source.AssociatedMedia,
                AssociatedOccurrences = source.AssociatedOccurrences,
                AssociatedReferences = source.AssociatedReferences,
                AssociatedSequences = source.AssociatedSequences,
                AssociatedTaxa = source.AssociatedTaxa,
                Behavior = source.Behavior?.Value,
                CatalogNumber = source.CatalogNumber,
                Disposition = source.Disposition,
                EstablishmentMeans = source.EstablishmentMeans?.Value,
                IndividualCount = source.IndividualCount,
                //IndividualID = source.IndividualId,
                LifeStage = source.LifeStage?.Value,
                OccurrenceID = source.OccurrenceId,
                OccurrenceRemarks = source.OccurrenceRemarks,
                OccurrenceStatus = source.OccurrenceStatus?.Value,
                OrganismQuantity = source.OrganismQuantity,
                OrganismQuantityType = source.OrganismQuantityUnit?.Value,
                OtherCatalogNumbers = source.OtherCatalogNumbers,
                RecordedBy = source.RecordedBy,
                ReproductiveCondition = source.ReproductiveCondition?.Value,
                Sex = source.Sex?.Value
            };
        }

        #endregion Occurrence

        #region Taxon

        /// <summary>
        ///     Cats processed taxon to darwin core
        /// </summary>
        /// <param name="taxon"></param>
        /// <returns></returns>
        public static DarwinCoreTaxon ToDarwinCore(this Taxon taxon)
        {
            return new DarwinCoreTaxon
            {
                AcceptedNameUsage = taxon.AcceptedNameUsage,
                AcceptedNameUsageID = taxon.AcceptedNameUsageId,
                Class = taxon.Class,
                Family = taxon.Family,
                Genus = taxon.Genus,
                HigherClassification = taxon.HigherClassification,
                InfraspecificEpithet = taxon.InfraspecificEpithet,
                Kingdom = taxon.Kingdom,
                NameAccordingTo = taxon.NameAccordingTo,
                NameAccordingToID = taxon.NameAccordingToId,
                NamePublishedIn = taxon.NamePublishedIn,
                NamePublishedInID = taxon.NamePublishedInId,
                NamePublishedInYear = taxon.NamePublishedInYear,
                NomenclaturalCode = taxon.NomenclaturalCode,
                NomenclaturalStatus = taxon.NomenclaturalStatus,
                Order = taxon.Order,
                OriginalNameUsage = taxon.OriginalNameUsage,
                OriginalNameUsageID = taxon.OriginalNameUsageId,
                ParentNameUsage = taxon.ParentNameUsage,
                ParentNameUsageID = taxon.ParentNameUsageId,
                Phylum = taxon.Phylum,
                ScientificName = taxon.ScientificName,
                ScientificNameAuthorship = taxon.ScientificNameAuthorship,
                ScientificNameID = taxon.ScientificNameId,
                SpecificEpithet = taxon.SpecificEpithet,
                Subgenus = taxon.Subgenus,
                TaxonConceptID = taxon.TaxonConceptId,
                TaxonID = taxon.TaxonId,
                VernacularName = taxon.VernacularName,
                TaxonRemarks = taxon.TaxonRemarks,
                TaxonRank = taxon.TaxonRank,
                TaxonomicStatus = taxon.TaxonomicStatus,
                VerbatimTaxonRank = taxon.VerbatimTaxonRank
            };
        }

        /// <summary>
        /// Return true if taxon is a bird
        /// </summary>
        /// <param name="taxon"></param>
        /// <returns></returns>
        public static bool IsBird(this Taxon taxon) => taxon?.Attributes?.OrganismGroup?.ToLower() == "fåglar";

        #endregion Taxon

        #region Sighting

        /// <summary>
        ///     Cast processed observation object to Darwin Core
        /// </summary>
        /// <param name="processedObservation"></param>
        /// <param name="fixSbdiArtportalenDwcObservation"></param>
        /// <returns></returns>
        public static DarwinCore ToDarwinCore(this Observation processedObservation, bool fixSbdiArtportalenDwcObservation = false)
        {
            if (processedObservation == null)
            {
                return null;
            }

            var dwc = new DarwinCore();
            dwc.AccessRights = processedObservation.AccessRights?.Value;
            dwc.BasisOfRecord = processedObservation.BasisOfRecord?.Value;
            dwc.BibliographicCitation = processedObservation.BibliographicCitation;
            dwc.CollectionCode = processedObservation.CollectionCode;
            dwc.CollectionID = processedObservation.CollectionId;
            dwc.DataGeneralizations = processedObservation.DataGeneralizations;
            dwc.DatasetID = processedObservation.DatasetId;
            dwc.DatasetName = processedObservation.DatasetName;
            dwc.Event = processedObservation.Event?.ToDarwinCore();
            dwc.Identification = processedObservation.Identification?.ToDarwinCore();
            dwc.InformationWithheld = processedObservation.InformationWithheld;
            if (fixSbdiArtportalenDwcObservation)
            {
                dwc.InstitutionCode = "SLU Artdatabanken";
                dwc.OwnerInstitutionCode = string.IsNullOrEmpty(processedObservation.InstitutionCode?.Value) ? "SLU Artdatabanken" : processedObservation.InstitutionCode?.Value;
            }
            else
            {
                dwc.InstitutionCode = string.IsNullOrEmpty(processedObservation.InstitutionCode?.Value) ? "SLU Artdatabanken" : processedObservation.InstitutionCode?.Value;
                dwc.OwnerInstitutionCode = processedObservation.OwnerInstitutionCode;
            }
            dwc.InstitutionID = processedObservation.InstitutionId;
            dwc.Language = processedObservation.Language;
            dwc.Location = processedObservation.Location?.ToDarwinCore();
            dwc.MeasurementOrFact = null;
            dwc.Modified = processedObservation.Modified;
            dwc.Occurrence = processedObservation.Occurrence?.ToDarwinCore();
            dwc.References = processedObservation.References;
            dwc.RightsHolder = processedObservation.RightsHolder;
            dwc.Taxon = processedObservation.Taxon.ToDarwinCore();
            dwc.Type = processedObservation.Type?.Value;

            return dwc;
        }

        /// <summary>
        ///     Cast processed Darwin Core objects to Darwin Core
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="fixSbdiArtportalenDwcObservation"></param>
        /// <returns></returns>
        public static IEnumerable<DarwinCore> ToDarwinCore(this IEnumerable<Observation> processedObservations, bool fixSbdiArtportalenDwcObservation = false)
        {
            return processedObservations?
                .Select(observation => observation.ToDarwinCore(fixSbdiArtportalenDwcObservation));
        }

        #endregion Sighting        
    }
}