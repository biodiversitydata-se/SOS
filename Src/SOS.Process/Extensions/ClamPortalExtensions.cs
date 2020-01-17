using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using  SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ClamPortal;

namespace SOS.Process.Extensions
{
    /// <summary>
    /// Entity extensions
    /// </summary>
    public static class ClamPortalExtensions
    {
        /// <summary>
        /// Cast clam observation verbatim to processed sighting
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static ProcessedSighting ToProcessed(this ClamObservationVerbatim verbatim, IDictionary<int, ProcessedTaxon> taxa)
        {
            taxa.TryGetValue(verbatim.DyntaxaTaxonId ?? -1, out var taxon);

            return new ProcessedSighting(DataProvider.ClamPortal)
            {
                AccessRights = verbatim.AccessRights,
                BasisOfRecord = verbatim.BasisOfRecord,
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProvider.ClamPortal.ToString()}",
                DatasetName = "Träd och musselportalen",
                Event = new ProcessedEvent
                {
                    EndDate = verbatim.ObservationDate.ToUniversalTime(),
                    SamplingProtocol = verbatim.SurveyMethod,
                    StartDate = verbatim.ObservationDate.ToUniversalTime(),
                    VerbatimEndDate = verbatim.ObservationDate,
                    VerbatimStartDate = verbatim.ObservationDate
                },
                Identification = new ProcessedIdentification
                {
                    VerificationStatus = string.IsNullOrEmpty(verbatim.IdentificationVerificationStatus) ? null : 
                        new Metadata(0){ Translations = new [] { new MetadataTranslation{ Culture = Cultures.sv_SE, Value = verbatim.IdentificationVerificationStatus }  } }  ,
                    UncertainDetermination = verbatim.UncertainDetermination != 0
                },
                Institution = string.IsNullOrEmpty(verbatim.InstitutionCode) ? null : 
                    new Metadata(0){ Translations = new []{ new MetadataTranslation{ Culture = Cultures.sv_SE, Value = verbatim.InstitutionCode } }},
                Language = verbatim.Language,
                Location = new ProcessedLocation
                {
                    Continent = "Europa",
                    CoordinatePrecision = verbatim.CoordinateUncertaintyInMeters,
                    CountryCode = verbatim.CountryCode,
                    DecimalLatitude = verbatim.DecimalLatitude,
                    DecimalLongitude = verbatim.DecimalLongitude,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    Id = verbatim.LocationId,
                    Locality = verbatim.Locality,
                    Remarks = verbatim.LocationRemarks,
                    MaximumDepthInMeters = verbatim.MaximumDepthInMeters,
                    VerbatimLatitude = verbatim.DecimalLatitude,
                    VerbatimLongitude = verbatim.DecimalLongitude,
                    VerbatimCoordinateSystem = "EPSG:4326",
                    WaterBody = verbatim.WaterBody
                },
                Modified = verbatim.Modified ?? DateTime.MinValue,
                Occurrence = new ProcessedOccurrence
                {
                    CatalogNumber = verbatim.CatalogNumber.ToString(),
                    Id = verbatim.OccurrenceId,
                    IndividualCount = verbatim.IndividualCount,
                    IsNaturalOccurrence = verbatim.IsNaturalOccurrence,
                    IsNeverFoundObservation = verbatim.IsNeverFoundObservation,
                    IsNotRediscoveredObservation = verbatim.IsNotRediscoveredObservation,
                    IsPositiveObservation = verbatim.IsPositiveObservation,
                    LifeStage = string.IsNullOrEmpty(verbatim.LifeStage) ? null :
                        new Metadata(0) { Translations = new[] { new MetadataTranslation { Culture = Cultures.sv_SE, Value = verbatim.LifeStage } } },
                    OrganismQuantity = verbatim.Quantity,
                    OrganismQuantityType = string.IsNullOrEmpty(verbatim.QuantityUnit) ? null :
                        new Metadata(0) { Translations = new[] { new MetadataTranslation { Culture = Cultures.sv_SE, Value = verbatim.QuantityUnit } } },
                    RecordedBy = verbatim.RecordedBy,
                    Remarks = verbatim.OccurrenceRemarks,
                    Status = verbatim.OccurrenceStatus
                },
                Projects = string.IsNullOrEmpty(verbatim.ProjectName) ? null : new[]
                {
                    new ProcessedProject
                    {
                        Name = verbatim.ProjectName
                    }
                },
                ReportedBy = verbatim.ReportedBy,
                ReportedDate = verbatim.ReportedDate,
                RightsHolder = verbatim.RightsHolder,
                Taxon = taxon
            };
        }

        /// <summary>
        /// Cast multiple clam observations to processed sightings
        /// </summary>
        /// <param name="verbatims"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static IEnumerable<ProcessedSighting> ToProcessed(this IEnumerable<ClamObservationVerbatim> verbatims, IDictionary<int, ProcessedTaxon> taxa)
        {
            return verbatims.Select(v => v.ToProcessed(taxa));
        }
    }
}
