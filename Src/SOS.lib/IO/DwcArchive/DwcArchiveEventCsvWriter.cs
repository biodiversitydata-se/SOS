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
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.IO.DwcArchive
{
    public class DwcArchiveEventCsvWriter : IDwcArchiveEventCsvWriter
    {
        private readonly ILogger<DwcArchiveEventCsvWriter> _logger;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;

        public DwcArchiveEventCsvWriter(
            IVocabularyValueResolver vocabularyValueResolver,
            ILogger<DwcArchiveEventCsvWriter> logger)
        {
            _vocabularyValueResolver = vocabularyValueResolver ?? throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> CreateEventCsvFileAsync(
            SearchFilterBase filter,
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedObservationRepository processedObservationRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var elasticRetrievalStopwatch = new Stopwatch();
                var csvWritingStopwatch = new Stopwatch();
                bool[] fieldsToWriteArray = FieldDescriptionHelper.CreateWriteFieldsArray(fieldDescriptions);
                elasticRetrievalStopwatch.Start();
                processedObservationRepository.LiveMode = true;
                var scrollResult = await processedObservationRepository.ScrollObservationsAsync(filter, null);
                elasticRetrievalStopwatch.Stop();
                using var csvFileHelper = new CsvFileHelper();
                csvFileHelper.InitializeWrite(stream, "\t");
                
                // Write header row
                WriteHeaderRow(csvFileHelper, fieldDescriptions);

                while (scrollResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    elasticRetrievalStopwatch.Start();
                    var processedObservations = scrollResult.Records.ToArray();
                    elasticRetrievalStopwatch.Stop();

                    // Convert observations to DwC format.
                    LocalDateTimeConverterHelper.ConvertToLocalTime(processedObservations);
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, Cultures.en_GB, true);
                    var dwcObservations = processedObservations.ToDarwinCore().ToArray();

                    // Write occurrence rows to CSV file.
                    csvWritingStopwatch.Start();
                    foreach (var dwcObservation in dwcObservations)
                    {
                        WriteEventRow(csvFileHelper, dwcObservation, fieldsToWriteArray);
                    }
                    await csvFileHelper.FlushAsync();
                    csvWritingStopwatch.Stop();

                    // Get next batch of observations.
                    elasticRetrievalStopwatch.Start();
                    scrollResult = await processedObservationRepository.ScrollObservationsAsync(filter, scrollResult.ScrollId);
                    elasticRetrievalStopwatch.Stop();
                }
                csvFileHelper.FinishWrite();

                stopwatch.Stop();
                _logger.LogInformation($"Occurrence CSV file created. Total time elapsed: {stopwatch.Elapsed.Duration()}. Elapsed time for CSV writing: {csvWritingStopwatch.Elapsed.Duration()}. Elapsed time for reading data from ElasticSearch: {elasticRetrievalStopwatch.Elapsed.Duration()}");
                return true;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation($"{nameof(CreateEventCsvFileAsync)} was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create occurrence CSV file.");
                throw;
            }
        }

        private static void WriteEventRow(
            CsvFileHelper csvFileHelper,
                    DarwinCore dwcObservation,
                    bool[] writeField)
        {
            if (writeField[(int)FieldDescriptionId.EventID]) csvFileHelper.WriteField(dwcObservation.Event.EventID);
            if (writeField[(int)FieldDescriptionId.ParentEventID]) csvFileHelper.WriteField(dwcObservation.Event.ParentEventID);
            if (writeField[(int)FieldDescriptionId.EventDate]) csvFileHelper.WriteField(dwcObservation.Event.EventDate);
            if (writeField[(int)FieldDescriptionId.VerbatimEventDate]) csvFileHelper.WriteField(dwcObservation.Event.VerbatimEventDate);
            if (writeField[(int)FieldDescriptionId.EventTime]) csvFileHelper.WriteField(dwcObservation.Event.EventTime);
            if (writeField[(int)FieldDescriptionId.EventRemarks]) csvFileHelper.WriteField(dwcObservation.Event.EventRemarks); 
            if (writeField[(int)FieldDescriptionId.FieldNotes]) csvFileHelper.WriteField(dwcObservation.Event.FieldNotes); 
            if (writeField[(int)FieldDescriptionId.FieldNumber]) csvFileHelper.WriteField(dwcObservation.Event.FieldNumber);
            if (writeField[(int)FieldDescriptionId.Habitat]) csvFileHelper.WriteField(dwcObservation.Event.Habitat);
            if (writeField[(int)FieldDescriptionId.SampleSizeValue]) csvFileHelper.WriteField(dwcObservation.Event.SampleSizeValue);
            if (writeField[(int)FieldDescriptionId.SampleSizeUnit]) csvFileHelper.WriteField(dwcObservation.Event.SampleSizeUnit);
            if (writeField[(int)FieldDescriptionId.SamplingEffort]) csvFileHelper.WriteField(dwcObservation.Event.SamplingEffort);
            if (writeField[(int)FieldDescriptionId.SamplingProtocol]) csvFileHelper.WriteField(dwcObservation.Event.SamplingProtocol);
            if (writeField[(int)FieldDescriptionId.Day]) csvFileHelper.WriteField(dwcObservation.Event.Day.HasValue ? dwcObservation.Event.Day.ToString() : null);
            if (writeField[(int)FieldDescriptionId.Month]) csvFileHelper.WriteField(dwcObservation.Event.Month.HasValue ? dwcObservation.Event.Month.ToString() : null);
            if (writeField[(int)FieldDescriptionId.Year]) csvFileHelper.WriteField(dwcObservation.Event.Year.HasValue ? dwcObservation.Event.Year.ToString() : null);
            if (writeField[(int)FieldDescriptionId.EndDayOfYear]) csvFileHelper.WriteField(dwcObservation.Event.EndDayOfYear.HasValue ? dwcObservation.Event.EndDayOfYear.ToString() : null);
            if (writeField[(int)FieldDescriptionId.StartDayOfYear]) csvFileHelper.WriteField(dwcObservation.Event.StartDayOfYear.HasValue ? dwcObservation.Event.StartDayOfYear.ToString() : null);
            if (writeField[(int)FieldDescriptionId.BasisOfRecord]) csvFileHelper.WriteField(dwcObservation.BasisOfRecord); 
            if (writeField[(int)FieldDescriptionId.BibliographicCitation]) csvFileHelper.WriteField(dwcObservation.BibliographicCitation); 
            if (writeField[(int)FieldDescriptionId.CollectionCode]) csvFileHelper.WriteField(dwcObservation.CollectionCode);
            if (writeField[(int)FieldDescriptionId.CollectionID]) csvFileHelper.WriteField(dwcObservation.CollectionID);
            if (writeField[(int)FieldDescriptionId.DataGeneralizations]) csvFileHelper.WriteField(dwcObservation.DataGeneralizations);
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
            if (writeField[(int)FieldDescriptionId.Continent]) csvFileHelper.WriteField(dwcObservation.Location.Continent); 
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

        private void WriteHeaderRow(CsvFileHelper csvFileHelper, IEnumerable<FieldDescription> fieldDescriptions)
        {
            foreach (var fieldDescription in fieldDescriptions)
            {
                csvFileHelper.WriteField(fieldDescription.Name);
            }

            csvFileHelper.NextRecord();
        }

        public async Task WriteHeaderlessEventCsvFileAsync(
            IEnumerable<DarwinCore> dwcObservations,
            StreamWriter streamWriter,
            IEnumerable<FieldDescription> fieldDescriptions)
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
                    WriteEventRow(csvFileHelper, dwcObservation, fieldsToWriteArray);
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