using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Export.Models;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Lib.IO.DwcArchive
{
    public class DwcArchiveOccurrenceCsvWriter : IDwcArchiveOccurrenceCsvWriter
    {
        private readonly ILogger<DwcArchiveOccurrenceCsvWriter> _logger;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;

        public DwcArchiveOccurrenceCsvWriter(
            IVocabularyValueResolver vocabularyValueResolver,
            ILogger<DwcArchiveOccurrenceCsvWriter> logger)
        {
            _vocabularyValueResolver = vocabularyValueResolver ?? throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> CreateOccurrenceCsvFileAsync(
            SearchFilterBase filter,
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedObservationRepository processedObservationRepository,
            IJobCancellationToken cancellationToken,
            bool leaveStreamOpen = false)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var elasticRetrievalStopwatch = new Stopwatch();
                var csvWritingStopwatch = new Stopwatch();
                int nrObservations = 0;
                bool[] fieldsToWriteArray = FieldDescriptionHelper.CreateWriteFieldsArray(fieldDescriptions);
                elasticRetrievalStopwatch.Start();
                processedObservationRepository.LiveMode = true;
                var searchResult = await processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter);
                elasticRetrievalStopwatch.Stop();

                using var csvFileHelper = new CsvFileHelper();
                csvFileHelper.InitializeWrite(stream, "\t", leaveStreamOpen);
                
                // Write header row
                WriteHeaderRow(csvFileHelper, fieldDescriptions);

                while (searchResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    elasticRetrievalStopwatch.Start();
                    var processedObservations = searchResult.Records.ToArray();
                    elasticRetrievalStopwatch.Stop();

                    // Convert observations to DwC format.
                    LocalDateTimeConverterHelper.ConvertToLocalTime(processedObservations);
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, Cultures.en_GB, true);
                    var dwcObservations = processedObservations.ToDarwinCore().ToArray();

                    // Write occurrence rows to CSV file.
                    csvWritingStopwatch.Start();
                    foreach (var dwcObservation in dwcObservations)
                    {
                        WriteOccurrenceRow(csvFileHelper, dwcObservation, fieldsToWriteArray);
                    }
                    nrObservations += dwcObservations.Length;
                    await csvFileHelper.FlushAsync();
                    csvWritingStopwatch.Stop();

                    // Get next batch of observations.
                    elasticRetrievalStopwatch.Start();
                    searchResult = await processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter, searchResult.PointInTimeId, searchResult.SearchAfter);
                    elasticRetrievalStopwatch.Stop();
                }
                csvFileHelper.FinishWrite();

                stopwatch.Stop();
                _logger.LogInformation($"Occurrence CSV file created. Total time elapsed: {stopwatch.Elapsed.Duration()}. Elapsed time for CSV writing: {csvWritingStopwatch.Elapsed.Duration()}. Elapsed time for reading data from ElasticSearch: {elasticRetrievalStopwatch.Elapsed.Duration()}");
                return nrObservations;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation($"{nameof(WriteHeaderlessOccurrenceCsvFileAsync)} was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create occurrence CSV file.");
                throw;
            }
        }

        /// <summary>
        /// Write occurrence record to CSV file.
        /// </summary>
        /// <param name="csvFileHelper"></param>
        /// <param name="dwcObservation"></param>
        /// <param name="writeField"></param>
        /// <param name="isEventCore"></param>
        /// <remarks>The fields must be written in correct order. FieldDescriptionId sorted ascending.</remarks>
        private static void WriteOccurrenceRow(
            CsvFileHelper csvFileHelper,
            DarwinCore dwcObservation,
            bool[] writeField,
            bool isEventCore = false)
        {
            if (isEventCore) csvFileHelper.WriteField(dwcObservation.Event.EventID);
            if (writeField[(int)FieldDescriptionId.OccurrenceID]) csvFileHelper.WriteField(dwcObservation.Occurrence.OccurrenceID);
            if (writeField[(int)FieldDescriptionId.BasisOfRecord]) csvFileHelper.WriteField(dwcObservation.BasisOfRecord); 
            if (writeField[(int)FieldDescriptionId.BibliographicCitation]) csvFileHelper.WriteField(dwcObservation.BibliographicCitation);
            if (writeField[(int)FieldDescriptionId.CollectionCode]) csvFileHelper.WriteField(dwcObservation.CollectionCode); 
            if (writeField[(int)FieldDescriptionId.CollectionID]) csvFileHelper.WriteField(dwcObservation.CollectionID);
            if (writeField[(int)FieldDescriptionId.DataGeneralizations]) csvFileHelper.WriteField(dwcObservation.DataGeneralizations);;
            if (writeField[(int)FieldDescriptionId.DatasetID]) csvFileHelper.WriteField(dwcObservation.DatasetID); 
            if (writeField[(int)FieldDescriptionId.DatasetName]) csvFileHelper.WriteField(dwcObservation.DatasetName); 
            if (writeField[(int)FieldDescriptionId.DynamicProperties]) csvFileHelper.WriteField(dwcObservation.DynamicProperties);
            if (writeField[(int)FieldDescriptionId.InformationWithheld]) csvFileHelper.WriteField(dwcObservation.InformationWithheld);
            if (writeField[(int)FieldDescriptionId.InstitutionCode]) csvFileHelper.WriteField(dwcObservation.InstitutionCode);
            if (writeField[(int)FieldDescriptionId.InstitutionID]) csvFileHelper.WriteField(dwcObservation.InstitutionID);
            if (writeField[(int)FieldDescriptionId.Language]) csvFileHelper.WriteField(dwcObservation.Language); 
            if (writeField[(int)FieldDescriptionId.License]) csvFileHelper.WriteField(dwcObservation.License);
            if (writeField[(int)FieldDescriptionId.Modified]) csvFileHelper.WriteField(dwcObservation.Modified?.ToString("s", CultureInfo.InvariantCulture)); 
            if (writeField[(int)FieldDescriptionId.OwnerInstitutionCode]) csvFileHelper.WriteField(dwcObservation.OwnerInstitutionCode);
            if (writeField[(int)FieldDescriptionId.References]) csvFileHelper.WriteField(dwcObservation.References); 
            if (writeField[(int)FieldDescriptionId.RightsHolder]) csvFileHelper.WriteField(dwcObservation.RightsHolder); 
            if (writeField[(int)FieldDescriptionId.Type]) csvFileHelper.WriteField(dwcObservation.Type); 
            if (writeField[(int)FieldDescriptionId.Day]) csvFileHelper.WriteField(dwcObservation.Event.Day.HasValue ? dwcObservation.Event.Day.ToString() : null); 
            if (writeField[(int)FieldDescriptionId.EndDayOfYear]) csvFileHelper.WriteField(dwcObservation.Event.EndDayOfYear.HasValue ? dwcObservation.Event.EndDayOfYear.ToString() : null);
            if (writeField[(int)FieldDescriptionId.EventDate]) csvFileHelper.WriteField(dwcObservation.Event.EventDate); 
            if (!isEventCore)
            {
                if (writeField[(int)FieldDescriptionId.EventID]) csvFileHelper.WriteField(dwcObservation.Event.EventID); 
            }
            if (writeField[(int)FieldDescriptionId.EventRemarks]) csvFileHelper.WriteField(dwcObservation.Event.EventRemarks); 
            if (writeField[(int)FieldDescriptionId.EventTime]) csvFileHelper.WriteField(dwcObservation.Event.EventTime); 
            if (writeField[(int)FieldDescriptionId.FieldNotes]) csvFileHelper.WriteField(dwcObservation.Event.FieldNotes); 
            if (writeField[(int)FieldDescriptionId.FieldNumber]) csvFileHelper.WriteField(dwcObservation.Event.FieldNumber); 
            if (writeField[(int)FieldDescriptionId.Habitat]) csvFileHelper.WriteField(dwcObservation.Event.Habitat); 
            if (writeField[(int)FieldDescriptionId.Month]) csvFileHelper.WriteField(dwcObservation.Event.Month.HasValue ? dwcObservation.Event.Month.ToString() : null); 
            if (writeField[(int)FieldDescriptionId.ParentEventID]) csvFileHelper.WriteField(dwcObservation.Event.ParentEventID); 
            if (writeField[(int)FieldDescriptionId.SampleSizeValue]) csvFileHelper.WriteField(dwcObservation.Event.SampleSizeValue);
            if (writeField[(int)FieldDescriptionId.SampleSizeUnit]) csvFileHelper.WriteField(dwcObservation.Event.SampleSizeUnit); 
            if (writeField[(int)FieldDescriptionId.SamplingEffort]) csvFileHelper.WriteField(dwcObservation.Event.SamplingEffort); 
            if (writeField[(int)FieldDescriptionId.SamplingProtocol]) csvFileHelper.WriteField(dwcObservation.Event.SamplingProtocol); 
            if (writeField[(int)FieldDescriptionId.StartDayOfYear]) csvFileHelper.WriteField(dwcObservation.Event.StartDayOfYear.HasValue ? dwcObservation.Event.StartDayOfYear.ToString() : null);
            if (writeField[(int)FieldDescriptionId.VerbatimEventDate]) csvFileHelper.WriteField(dwcObservation.Event.VerbatimEventDate);
            if (writeField[(int)FieldDescriptionId.Year]) csvFileHelper.WriteField(dwcObservation.Event.Year.HasValue ? dwcObservation.Event.Year.ToString() : null); 
            if (writeField[(int)FieldDescriptionId.DateIdentified]) csvFileHelper.WriteField(dwcObservation.Identification.DateIdentified); 
            if (writeField[(int)FieldDescriptionId.IdentificationID]) csvFileHelper.WriteField(dwcObservation.Identification.IdentificationID);
            if (writeField[(int)FieldDescriptionId.IdentificationQualifier]) csvFileHelper.WriteField(dwcObservation.Identification.IdentificationQualifier);
            if (writeField[(int)FieldDescriptionId.IdentificationReferences]) csvFileHelper.WriteField(dwcObservation.Identification.IdentificationReferences);
            if (writeField[(int)FieldDescriptionId.IdentificationRemarks]) csvFileHelper.WriteField(dwcObservation.Identification.IdentificationRemarks);
            if (writeField[(int)FieldDescriptionId.IdentificationVerificationStatus]) csvFileHelper.WriteField(dwcObservation.Identification.IdentificationVerificationStatus);
            if (writeField[(int)FieldDescriptionId.IdentifiedBy]) csvFileHelper.WriteField(dwcObservation.Identification.IdentifiedBy);
            if (writeField[(int)FieldDescriptionId.TypeStatus]) csvFileHelper.WriteField(dwcObservation.Identification.TypeStatus); 
            if (writeField[(int)FieldDescriptionId.Continent]) csvFileHelper.WriteField(dwcObservation.Location.Continent);
            if (writeField[(int)FieldDescriptionId.CoordinatePrecision]) csvFileHelper.WriteField(dwcObservation.Location.CoordinatePrecision); 
            if (writeField[(int)FieldDescriptionId.CoordinateUncertaintyInMeters]) csvFileHelper.WriteField(GetCoordinateUncertaintyInMetersValue(dwcObservation.Location.CoordinateUncertaintyInMeters));
            if (writeField[(int)FieldDescriptionId.Country]) csvFileHelper.WriteField(dwcObservation.Location.Country);
            if (writeField[(int)FieldDescriptionId.CountryCode]) csvFileHelper.WriteField(dwcObservation.Location.CountryCode); 
            if (writeField[(int)FieldDescriptionId.County]) csvFileHelper.WriteField(dwcObservation.Location.County);
            if (writeField[(int)FieldDescriptionId.DecimalLatitude]) csvFileHelper.WriteField(dwcObservation.Location.DecimalLatitude?.ToString("F5", CultureInfo.InvariantCulture));
            if (writeField[(int)FieldDescriptionId.DecimalLongitude]) csvFileHelper.WriteField(dwcObservation.Location.DecimalLongitude?.ToString("F5", CultureInfo.InvariantCulture));
            if (writeField[(int)FieldDescriptionId.FootprintSpatialFit]) csvFileHelper.WriteField(dwcObservation.Location.FootprintSpatialFit);
            if (writeField[(int)FieldDescriptionId.FootprintSRS]) csvFileHelper.WriteField(dwcObservation.Location.FootprintSRS);
            if (writeField[(int)FieldDescriptionId.FootprintWKT]) csvFileHelper.WriteField(dwcObservation.Location.FootprintWKT);
            if (writeField[(int)FieldDescriptionId.GeodeticDatum]) csvFileHelper.WriteField(dwcObservation.Location.GeodeticDatum);
            if (writeField[(int)FieldDescriptionId.GeoreferencedBy]) csvFileHelper.WriteField(dwcObservation.Location.GeoreferencedBy);
            if (writeField[(int)FieldDescriptionId.GeoreferencedDate]) csvFileHelper.WriteField(dwcObservation.Location.GeoreferencedDate);
            if (writeField[(int)FieldDescriptionId.GeoreferenceProtocol]) csvFileHelper.WriteField(dwcObservation.Location.GeoreferenceProtocol); 
            if (writeField[(int)FieldDescriptionId.GeoreferenceRemarks]) csvFileHelper.WriteField(dwcObservation.Location.GeoreferenceRemarks); 
            if (writeField[(int)FieldDescriptionId.GeoreferenceSources]) csvFileHelper.WriteField(dwcObservation.Location.GeoreferenceSources); 
            if (writeField[(int)FieldDescriptionId.GeoreferenceVerificationStatus]) csvFileHelper.WriteField(dwcObservation.Location.GeoreferenceVerificationStatus);
            if (writeField[(int)FieldDescriptionId.HigherGeography]) csvFileHelper.WriteField(dwcObservation.Location.HigherGeography); 
            if (writeField[(int)FieldDescriptionId.HigherGeographyID]) csvFileHelper.WriteField(dwcObservation.Location.HigherGeographyID); 
            if (writeField[(int)FieldDescriptionId.Island]) csvFileHelper.WriteField(dwcObservation.Location.Island); 
            if (writeField[(int)FieldDescriptionId.IslandGroup]) csvFileHelper.WriteField(dwcObservation.Location.IslandGroup);
            if (writeField[(int)FieldDescriptionId.Locality]) csvFileHelper.WriteField(dwcObservation.Location.Locality);
            if (writeField[(int)FieldDescriptionId.LocationAccordingTo]) csvFileHelper.WriteField(dwcObservation.Location.LocationAccordingTo);
            if (writeField[(int)FieldDescriptionId.LocationID]) csvFileHelper.WriteField(dwcObservation.Location.LocationID); 
            if (writeField[(int)FieldDescriptionId.LocationRemarks]) csvFileHelper.WriteField(dwcObservation.Location.LocationRemarks);
            if (writeField[(int)FieldDescriptionId.MaximumDepthInMeters]) csvFileHelper.WriteField(dwcObservation.Location.MaximumDepthInMeters); 
            if (writeField[(int)FieldDescriptionId.MaximumDistanceAboveSurfaceInMeters]) csvFileHelper.WriteField(dwcObservation.Location.MaximumDistanceAboveSurfaceInMeters); 
            if (writeField[(int)FieldDescriptionId.MaximumElevationInMeters]) csvFileHelper.WriteField(dwcObservation.Location.MaximumElevationInMeters);
            if (writeField[(int)FieldDescriptionId.MinimumDepthInMeters]) csvFileHelper.WriteField(dwcObservation.Location.MinimumDepthInMeters);
            if (writeField[(int)FieldDescriptionId.MinimumDistanceAboveSurfaceInMeters]) csvFileHelper.WriteField(dwcObservation.Location.MinimumDistanceAboveSurfaceInMeters);
            if (writeField[(int)FieldDescriptionId.MinimumElevationInMeters]) csvFileHelper.WriteField(dwcObservation.Location.MinimumElevationInMeters); 
            if (writeField[(int)FieldDescriptionId.Municipality]) csvFileHelper.WriteField(dwcObservation.Location.Municipality); 
            if (writeField[(int)FieldDescriptionId.PointRadiusSpatialFit]) csvFileHelper.WriteField(dwcObservation.Location.PointRadiusSpatialFit); 
            if (writeField[(int)FieldDescriptionId.StateProvince]) csvFileHelper.WriteField(dwcObservation.Location.StateProvince);
            if (writeField[(int)FieldDescriptionId.WaterBody]) csvFileHelper.WriteField(dwcObservation.Location.WaterBody);
            if (writeField[(int)FieldDescriptionId.VerbatimCoordinates]) csvFileHelper.WriteField(dwcObservation.Location.VerbatimCoordinates); 
            if (writeField[(int)FieldDescriptionId.VerbatimCoordinateSystem]) csvFileHelper.WriteField(dwcObservation.Location.VerbatimCoordinateSystem);
            if (writeField[(int)FieldDescriptionId.VerbatimDepth]) csvFileHelper.WriteField(dwcObservation.Location.VerbatimDepth);
            if (writeField[(int)FieldDescriptionId.VerbatimElevation]) csvFileHelper.WriteField(dwcObservation.Location.VerbatimElevation); 
            if (writeField[(int)FieldDescriptionId.VerbatimLatitude]) csvFileHelper.WriteField(dwcObservation.Location.VerbatimLatitude); 
            if (writeField[(int)FieldDescriptionId.VerbatimLocality]) csvFileHelper.WriteField(dwcObservation.Location.VerbatimLocality);
            if (writeField[(int)FieldDescriptionId.VerbatimLongitude]) csvFileHelper.WriteField(dwcObservation.Location.VerbatimLongitude); 
            if (writeField[(int)FieldDescriptionId.VerbatimSRS]) csvFileHelper.WriteField(dwcObservation.Location.VerbatimSRS); 
            if (writeField[(int)FieldDescriptionId.AssociatedMedia]) csvFileHelper.WriteField(dwcObservation.Occurrence.AssociatedMedia); 
            if (writeField[(int)FieldDescriptionId.AssociatedReferences]) csvFileHelper.WriteField(dwcObservation.Occurrence.AssociatedReferences); 
            if (writeField[(int)FieldDescriptionId.AssociatedSequences]) csvFileHelper.WriteField(dwcObservation.Occurrence.AssociatedSequences);
            if (writeField[(int)FieldDescriptionId.AssociatedTaxa]) csvFileHelper.WriteField(dwcObservation.Occurrence.AssociatedTaxa);
            if (writeField[(int)FieldDescriptionId.Behavior]) csvFileHelper.WriteField(dwcObservation.Occurrence.Behavior);
            if (writeField[(int)FieldDescriptionId.CatalogNumber]) csvFileHelper.WriteField(dwcObservation.Occurrence.CatalogNumber);
            if (writeField[(int)FieldDescriptionId.Disposition]) csvFileHelper.WriteField(dwcObservation.Occurrence.Disposition);
            if (writeField[(int)FieldDescriptionId.EstablishmentMeans]) csvFileHelper.WriteField(dwcObservation.Occurrence.EstablishmentMeans);
            if (writeField[(int)FieldDescriptionId.IndividualCount]) csvFileHelper.WriteField(dwcObservation.Occurrence.IndividualCount); 
            if (writeField[(int)FieldDescriptionId.LifeStage]) csvFileHelper.WriteField(dwcObservation.Occurrence.LifeStage);
            if (writeField[(int)FieldDescriptionId.AccessRights]) csvFileHelper.WriteField(dwcObservation.AccessRights);
            if (writeField[(int)FieldDescriptionId.OccurrenceRemarks]) csvFileHelper.WriteField(dwcObservation.Occurrence.OccurrenceRemarks);
            if (writeField[(int)FieldDescriptionId.OccurrenceStatus]) csvFileHelper.WriteField(dwcObservation.Occurrence.OccurrenceStatus);
            if (writeField[(int)FieldDescriptionId.OrganismQuantity]) csvFileHelper.WriteField(dwcObservation.Occurrence.OrganismQuantity); 
            if (writeField[(int)FieldDescriptionId.OrganismQuantityType]) csvFileHelper.WriteField(dwcObservation.Occurrence.OrganismQuantityType);
            if (writeField[(int)FieldDescriptionId.OtherCatalogNumbers]) csvFileHelper.WriteField(dwcObservation.Occurrence.OtherCatalogNumbers);
            if (writeField[(int)FieldDescriptionId.Preparations]) csvFileHelper.WriteField(dwcObservation.Occurrence.Preparations);
            if (writeField[(int)FieldDescriptionId.RecordedBy]) csvFileHelper.WriteField(dwcObservation.Occurrence.RecordedBy);
            if (writeField[(int)FieldDescriptionId.RecordNumber]) csvFileHelper.WriteField(dwcObservation.Occurrence.RecordNumber);
            if (writeField[(int)FieldDescriptionId.ReproductiveCondition]) csvFileHelper.WriteField(dwcObservation.Occurrence.ReproductiveCondition);
            if (writeField[(int)FieldDescriptionId.Sex]) csvFileHelper.WriteField(dwcObservation.Occurrence.Sex);
            if (writeField[(int)FieldDescriptionId.AcceptedNameUsage]) csvFileHelper.WriteField(dwcObservation.Taxon.AcceptedNameUsage); 
            if (writeField[(int)FieldDescriptionId.AcceptedNameUsageID]) csvFileHelper.WriteField(dwcObservation.Taxon.AcceptedNameUsageID); 
            if (writeField[(int)FieldDescriptionId.Class]) csvFileHelper.WriteField(dwcObservation.Taxon.Class);
            if (writeField[(int)FieldDescriptionId.Family]) csvFileHelper.WriteField(dwcObservation.Taxon.Family);
            if (writeField[(int)FieldDescriptionId.Genus]) csvFileHelper.WriteField(dwcObservation.Taxon.Genus);
            if (writeField[(int)FieldDescriptionId.HigherClassification]) csvFileHelper.WriteField(dwcObservation.Taxon.HigherClassification); 
            if (writeField[(int)FieldDescriptionId.InfraspecificEpithet]) csvFileHelper.WriteField(dwcObservation.Taxon.InfraspecificEpithet);
            if (writeField[(int)FieldDescriptionId.Kingdom]) csvFileHelper.WriteField(dwcObservation.Taxon.Kingdom);
            if (writeField[(int)FieldDescriptionId.NameAccordingTo]) csvFileHelper.WriteField(dwcObservation.Taxon.NameAccordingTo); 
            if (writeField[(int)FieldDescriptionId.NameAccordingToID]) csvFileHelper.WriteField(dwcObservation.Taxon.NameAccordingToID); 
            if (writeField[(int)FieldDescriptionId.NamePublishedIn]) csvFileHelper.WriteField(dwcObservation.Taxon.NamePublishedIn); 
            if (writeField[(int)FieldDescriptionId.NamePublishedInID]) csvFileHelper.WriteField(dwcObservation.Taxon.NamePublishedInID); 
            if (writeField[(int)FieldDescriptionId.NamePublishedInYear]) csvFileHelper.WriteField(dwcObservation.Taxon.NamePublishedInYear); 
            if (writeField[(int)FieldDescriptionId.NomenclaturalCode]) csvFileHelper.WriteField(dwcObservation.Taxon.NomenclaturalCode); 
            if (writeField[(int)FieldDescriptionId.NomenclaturalStatus]) csvFileHelper.WriteField(dwcObservation.Taxon.NomenclaturalStatus); 
            if (writeField[(int)FieldDescriptionId.Order]) csvFileHelper.WriteField(dwcObservation.Taxon.Order);
            if (writeField[(int)FieldDescriptionId.OriginalNameUsage]) csvFileHelper.WriteField(dwcObservation.Taxon.OriginalNameUsage); 
            if (writeField[(int)FieldDescriptionId.OriginalNameUsageID]) csvFileHelper.WriteField(dwcObservation.Taxon.OriginalNameUsageID); 
            if (writeField[(int)FieldDescriptionId.ParentNameUsage]) csvFileHelper.WriteField(dwcObservation.Taxon.ParentNameUsage);
            if (writeField[(int)FieldDescriptionId.ParentNameUsageID]) csvFileHelper.WriteField(dwcObservation.Taxon.ParentNameUsageID);
            if (writeField[(int)FieldDescriptionId.Phylum]) csvFileHelper.WriteField(dwcObservation.Taxon.Phylum); 
            if (writeField[(int)FieldDescriptionId.ScientificName]) csvFileHelper.WriteField(dwcObservation.Taxon.ScientificName);
            if (writeField[(int)FieldDescriptionId.ScientificNameAuthorship]) csvFileHelper.WriteField(dwcObservation.Taxon.ScientificNameAuthorship); 
            if (writeField[(int)FieldDescriptionId.ScientificNameID]) csvFileHelper.WriteField(dwcObservation.Taxon.ScientificNameID);
            if (writeField[(int)FieldDescriptionId.SpecificEpithet]) csvFileHelper.WriteField(dwcObservation.Taxon.SpecificEpithet); 
            if (writeField[(int)FieldDescriptionId.Subgenus]) csvFileHelper.WriteField(dwcObservation.Taxon.Subgenus); 
            if (writeField[(int)FieldDescriptionId.TaxonConceptID]) csvFileHelper.WriteField(dwcObservation.Taxon.TaxonConceptID); 
            if (writeField[(int)FieldDescriptionId.TaxonID]) csvFileHelper.WriteField(dwcObservation.Taxon.TaxonID); 
            if (writeField[(int)FieldDescriptionId.TaxonomicStatus]) csvFileHelper.WriteField(dwcObservation.Taxon.TaxonomicStatus); 
            if (writeField[(int)FieldDescriptionId.TaxonRank]) csvFileHelper.WriteField(dwcObservation.Taxon.TaxonRank);
            if (writeField[(int)FieldDescriptionId.TaxonRemarks]) csvFileHelper.WriteField(dwcObservation.Taxon.TaxonRemarks); 
            if (writeField[(int)FieldDescriptionId.VerbatimTaxonRank]) csvFileHelper.WriteField(dwcObservation.Taxon.VerbatimTaxonRank); 
            if (writeField[(int)FieldDescriptionId.VernacularName]) csvFileHelper.WriteField(dwcObservation.Taxon.VernacularName);
            if (writeField[(int)FieldDescriptionId.Bed]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.Bed); 
            if (writeField[(int)FieldDescriptionId.EarliestAgeOrLowestStage]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.EarliestAgeOrLowestStage);
            if (writeField[(int)FieldDescriptionId.EarliestEonOrLowestEonothem]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.EarliestEonOrLowestEonothem);
            if (writeField[(int)FieldDescriptionId.EarliestEpochOrLowestSeries]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.EarliestEpochOrLowestSeries); 
            if (writeField[(int)FieldDescriptionId.EarliestEraOrLowestErathem]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.EarliestEraOrLowestErathem); 
            if (writeField[(int)FieldDescriptionId.EarliestPeriodOrLowestSystem]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.EarliestPeriodOrLowestSystem);
            if (writeField[(int)FieldDescriptionId.Formation]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.Formation);
            if (writeField[(int)FieldDescriptionId.GeologicalContextID]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.GeologicalContextID); 
            if (writeField[(int)FieldDescriptionId.Group]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.Group);
            if (writeField[(int)FieldDescriptionId.HighestBiostratigraphicZone]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.HighestBiostratigraphicZone);
            if (writeField[(int)FieldDescriptionId.LatestAgeOrHighestStage]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.LatestAgeOrHighestStage); 
            if (writeField[(int)FieldDescriptionId.LatestEonOrHighestEonothem]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.LatestEonOrHighestEonothem);
            if (writeField[(int)FieldDescriptionId.LatestEpochOrHighestSeries]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.LatestEpochOrHighestSeries); 
            if (writeField[(int)FieldDescriptionId.LatestEraOrHighestErathem]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.LatestEraOrHighestErathem);
            if (writeField[(int)FieldDescriptionId.LatestPeriodOrHighestSystem]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.LatestPeriodOrHighestSystem);
            if (writeField[(int)FieldDescriptionId.LithostratigraphicTerms]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.LithostratigraphicTerms);
            if (writeField[(int)FieldDescriptionId.LowestBiostratigraphicZone]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.LowestBiostratigraphicZone);
            if (writeField[(int)FieldDescriptionId.Member]) csvFileHelper.WriteField(dwcObservation.GeologicalContext?.Member);
            if (writeField[(int)FieldDescriptionId.MaterialSampleID]) csvFileHelper.WriteField(dwcObservation.MaterialSample?.MaterialSampleID);

            csvFileHelper.NextRecord();
        }

        private static string GetCoordinateUncertaintyInMetersValue(int? coordinateUncertaintyInMeters)
        {
            if (!coordinateUncertaintyInMeters.HasValue) return null;
            if (coordinateUncertaintyInMeters < 0)
            {
                return null;
            }
            if (coordinateUncertaintyInMeters.Value == 0)
            {
                return 1.ToString();
            }

            return coordinateUncertaintyInMeters.Value.ToString();
        }

        public void WriteHeaderRow(CsvFileHelper csvFileHelper, IEnumerable<FieldDescription> fieldDescriptions)
        {
            foreach (var fieldDescription in fieldDescriptions)
            {
                csvFileHelper.WriteField(fieldDescription.Name);
            }

            csvFileHelper.NextRecord();
        }

        public async Task WriteHeaderlessOccurrenceCsvFileAsync(
            IEnumerable<DarwinCore> dwcObservations,
            StreamWriter streamWriter,
            IEnumerable<FieldDescription> fieldDescriptions,
            bool isEventCore = false)
        {
            try
            {
                bool[] fieldsToWriteArray = FieldDescriptionHelper.CreateWriteFieldsArray(fieldDescriptions);
                //await using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
                using var csvFileHelper = new CsvFileHelper();
                csvFileHelper.InitializeWrite(streamWriter, "\t");

                // Write occurrence rows to CSV file.
                foreach (var dwcObservation in dwcObservations)
                {
                    WriteOccurrenceRow(csvFileHelper, dwcObservation, fieldsToWriteArray, isEventCore);
                }
                csvFileHelper.FinishWrite();

                //_logger.LogInformation($"Occurrence CSV file created. Total time elapsed: {stopwatch.Elapsed.Duration()}. Elapsed time for CSV writing: {csvWritingStopwatch.Elapsed.Duration()}. Elapsed time for reading data from ElasticSearch: {elasticRetrievalStopwatch.Elapsed.Duration()}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create occurrence CSV file.");
                throw;
            }
        }
    }
}