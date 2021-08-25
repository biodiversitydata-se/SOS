using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using NReco.Csv;
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
            FilterBase filter,
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
                await using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
                var csvWriter = new NReco.Csv.CsvWriter(streamWriter, "\t");

                // Write header row
                WriteHeaderRow(csvWriter, fieldDescriptions);

                while (scrollResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    elasticRetrievalStopwatch.Start();
                    var processedObservations = scrollResult.Records.ToArray();
                    elasticRetrievalStopwatch.Stop();

                    // Convert observations to DwC format.
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, Cultures.en_GB, true);
                    var dwcObservations = processedObservations.ToDarwinCore().ToArray();

                    // Write occurrence rows to CSV file.
                    csvWritingStopwatch.Start();
                    foreach (var dwcObservation in dwcObservations)
                    {
                        WriteEventRow(csvWriter, dwcObservation, fieldsToWriteArray);
                    }
                    await streamWriter.FlushAsync();
                    csvWritingStopwatch.Stop();

                    // Get next batch of observations.
                    elasticRetrievalStopwatch.Start();
                    scrollResult = await processedObservationRepository.ScrollObservationsAsync(filter, scrollResult.ScrollId);
                    elasticRetrievalStopwatch.Stop();
                }

                stopwatch.Stop();
                _logger.LogInformation($"Occurrence CSV file created. Total time elapsed: {stopwatch.Elapsed.Duration()}. Elapsed time for CSV writing: {csvWritingStopwatch.Elapsed.Duration()}. Elapsed time for reading data from ElasticSearch: {elasticRetrievalStopwatch.Elapsed.Duration()}");
                return true;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation($"{nameof(WriteHeaderlessEventCsvFileAsync)} was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create occurrence CSV file.");
                throw;
            }
        }

        private static void WriteEventRow(
                    NReco.Csv.CsvWriter csvWriter,
                    DarwinCore dwcObservation,
                    bool[] writeField)
        {
            if (writeField[(int)FieldDescriptionId.EventID]) WriteField(csvWriter, dwcObservation.Event.EventID);
            if (writeField[(int)FieldDescriptionId.ParentEventID]) WriteField(csvWriter, dwcObservation.Event.ParentEventID);
            if (writeField[(int)FieldDescriptionId.EventDate]) WriteField(csvWriter, dwcObservation.Event.EventDate);
            if (writeField[(int)FieldDescriptionId.VerbatimEventDate]) WriteField(csvWriter, dwcObservation.Event.VerbatimEventDate);
            if (writeField[(int)FieldDescriptionId.EventTime]) WriteField(csvWriter, dwcObservation.Event.EventTime);
            if (writeField[(int)FieldDescriptionId.EventRemarks]) WriteField(csvWriter, dwcObservation.Event.EventRemarks);
            if (writeField[(int)FieldDescriptionId.FieldNotes]) WriteField(csvWriter, dwcObservation.Event.FieldNotes);
            if (writeField[(int)FieldDescriptionId.FieldNumber]) WriteField(csvWriter, dwcObservation.Event.FieldNumber);
            if (writeField[(int)FieldDescriptionId.Habitat]) WriteField(csvWriter, dwcObservation.Event.Habitat);
            if (writeField[(int)FieldDescriptionId.SampleSizeValue]) WriteField(csvWriter, dwcObservation.Event.SampleSizeValue);
            if (writeField[(int)FieldDescriptionId.SampleSizeUnit]) WriteField(csvWriter, dwcObservation.Event.SampleSizeUnit);
            if (writeField[(int)FieldDescriptionId.SamplingEffort]) WriteField(csvWriter, dwcObservation.Event.SamplingEffort);
            if (writeField[(int)FieldDescriptionId.SamplingProtocol]) WriteField(csvWriter, dwcObservation.Event.SamplingProtocol);
            if (writeField[(int)FieldDescriptionId.Day]) WriteField(csvWriter, dwcObservation.Event.Day.HasValue ? dwcObservation.Event.Day.ToString() : null);
            if (writeField[(int)FieldDescriptionId.Month]) WriteField(csvWriter, dwcObservation.Event.Month.HasValue ? dwcObservation.Event.Month.ToString() : null);
            if (writeField[(int)FieldDescriptionId.Year]) WriteField(csvWriter, dwcObservation.Event.Year.HasValue ? dwcObservation.Event.Year.ToString() : null);
            if (writeField[(int)FieldDescriptionId.EndDayOfYear]) WriteField(csvWriter, dwcObservation.Event.EndDayOfYear.HasValue ? dwcObservation.Event.EndDayOfYear.ToString() : null);
            if (writeField[(int)FieldDescriptionId.StartDayOfYear]) WriteField(csvWriter, dwcObservation.Event.StartDayOfYear.HasValue ? dwcObservation.Event.StartDayOfYear.ToString() : null);
            if (writeField[(int)FieldDescriptionId.BasisOfRecord]) WriteField(csvWriter, dwcObservation.BasisOfRecord);
            if (writeField[(int)FieldDescriptionId.BibliographicCitation]) WriteField(csvWriter, dwcObservation.BibliographicCitation);
            if (writeField[(int)FieldDescriptionId.CollectionCode]) WriteField(csvWriter, dwcObservation.CollectionCode);
            if (writeField[(int)FieldDescriptionId.CollectionID]) WriteField(csvWriter, dwcObservation.CollectionID);
            if (writeField[(int)FieldDescriptionId.DataGeneralizations]) WriteField(csvWriter, dwcObservation.DataGeneralizations);
            if (writeField[(int)FieldDescriptionId.DatasetID]) WriteField(csvWriter, dwcObservation.DatasetID);
            if (writeField[(int)FieldDescriptionId.DatasetName]) WriteField(csvWriter, dwcObservation.DatasetName);
            if (writeField[(int)FieldDescriptionId.DynamicProperties]) WriteField(csvWriter, dwcObservation.DynamicProperties);
            if (writeField[(int)FieldDescriptionId.InformationWithheld]) WriteField(csvWriter, dwcObservation.InformationWithheld);
            if (writeField[(int)FieldDescriptionId.InstitutionCode]) WriteField(csvWriter, dwcObservation.InstitutionCode);
            if (writeField[(int)FieldDescriptionId.InstitutionID]) WriteField(csvWriter, dwcObservation.InstitutionID);
            if (writeField[(int)FieldDescriptionId.Language]) WriteField(csvWriter, dwcObservation.Language);
            if (writeField[(int)FieldDescriptionId.License]) WriteField(csvWriter, dwcObservation.License);
            if (writeField[(int)FieldDescriptionId.Modified]) WriteField(csvWriter, dwcObservation.Modified?.ToString("s", CultureInfo.InvariantCulture));
            if (writeField[(int)FieldDescriptionId.OwnerInstitutionCode]) WriteField(csvWriter, dwcObservation.OwnerInstitutionCode);
            if (writeField[(int)FieldDescriptionId.References]) WriteField(csvWriter, dwcObservation.References);
            if (writeField[(int)FieldDescriptionId.RightsHolder]) WriteField(csvWriter, dwcObservation.RightsHolder);
            if (writeField[(int)FieldDescriptionId.Type]) WriteField(csvWriter, dwcObservation.Type);
            if (writeField[(int)FieldDescriptionId.CoordinatePrecision]) WriteField(csvWriter, dwcObservation.Location.CoordinatePrecision);
            if (writeField[(int)FieldDescriptionId.CoordinateUncertaintyInMeters]) WriteField(csvWriter, GetCoordinateUncertaintyInMetersValue(dwcObservation.Location.CoordinateUncertaintyInMeters));
            if (writeField[(int)FieldDescriptionId.Country]) WriteField(csvWriter, dwcObservation.Location.Country);
            if (writeField[(int)FieldDescriptionId.CountryCode]) WriteField(csvWriter, dwcObservation.Location.CountryCode);
            if (writeField[(int)FieldDescriptionId.County]) WriteField(csvWriter, dwcObservation.Location.County);
            if (writeField[(int)FieldDescriptionId.DecimalLatitude]) WriteField(csvWriter, dwcObservation.Location.DecimalLatitude?.ToString("F5", CultureInfo.InvariantCulture));
            if (writeField[(int)FieldDescriptionId.DecimalLongitude]) WriteField(csvWriter, dwcObservation.Location.DecimalLongitude?.ToString("F5", CultureInfo.InvariantCulture));
            if (writeField[(int)FieldDescriptionId.FootprintSpatialFit]) WriteField(csvWriter, dwcObservation.Location.FootprintSpatialFit);
            if (writeField[(int)FieldDescriptionId.FootprintSRS]) WriteField(csvWriter, dwcObservation.Location.FootprintSRS);
            if (writeField[(int)FieldDescriptionId.FootprintWKT]) WriteField(csvWriter, dwcObservation.Location.FootprintWKT);
            if (writeField[(int)FieldDescriptionId.GeodeticDatum]) WriteField(csvWriter, dwcObservation.Location.GeodeticDatum);
            if (writeField[(int)FieldDescriptionId.GeoreferencedBy]) WriteField(csvWriter, dwcObservation.Location.GeoreferencedBy);
            if (writeField[(int)FieldDescriptionId.GeoreferencedDate]) WriteField(csvWriter, dwcObservation.Location.GeoreferencedDate);
            if (writeField[(int)FieldDescriptionId.GeoreferenceProtocol]) WriteField(csvWriter, dwcObservation.Location.GeoreferenceProtocol);
            if (writeField[(int)FieldDescriptionId.GeoreferenceRemarks]) WriteField(csvWriter, dwcObservation.Location.GeoreferenceRemarks);
            if (writeField[(int)FieldDescriptionId.GeoreferenceSources]) WriteField(csvWriter, dwcObservation.Location.GeoreferenceSources);
            if (writeField[(int)FieldDescriptionId.GeoreferenceVerificationStatus]) WriteField(csvWriter, dwcObservation.Location.GeoreferenceVerificationStatus);
            if (writeField[(int)FieldDescriptionId.HigherGeography]) WriteField(csvWriter, dwcObservation.Location.HigherGeography);
            if (writeField[(int)FieldDescriptionId.HigherGeographyID]) WriteField(csvWriter, dwcObservation.Location.HigherGeographyID);
            if (writeField[(int)FieldDescriptionId.Continent]) WriteField(csvWriter, dwcObservation.Location.Continent);
            if (writeField[(int)FieldDescriptionId.Island]) WriteField(csvWriter, dwcObservation.Location.Island);
            if (writeField[(int)FieldDescriptionId.IslandGroup]) WriteField(csvWriter, dwcObservation.Location.IslandGroup);
            if (writeField[(int)FieldDescriptionId.Locality]) WriteField(csvWriter, dwcObservation.Location.Locality);
            if (writeField[(int)FieldDescriptionId.LocationAccordingTo]) WriteField(csvWriter, dwcObservation.Location.LocationAccordingTo);
            if (writeField[(int)FieldDescriptionId.LocationID]) WriteField(csvWriter, dwcObservation.Location.LocationID);
            if (writeField[(int)FieldDescriptionId.LocationRemarks]) WriteField(csvWriter, dwcObservation.Location.LocationRemarks);
            if (writeField[(int)FieldDescriptionId.MaximumDepthInMeters]) WriteField(csvWriter, dwcObservation.Location.MaximumDepthInMeters);
            if (writeField[(int)FieldDescriptionId.MaximumDistanceAboveSurfaceInMeters]) WriteField(csvWriter, dwcObservation.Location.MaximumDistanceAboveSurfaceInMeters);
            if (writeField[(int)FieldDescriptionId.MaximumElevationInMeters]) WriteField(csvWriter, dwcObservation.Location.MaximumElevationInMeters);
            if (writeField[(int)FieldDescriptionId.MinimumDepthInMeters]) WriteField(csvWriter, dwcObservation.Location.MinimumDepthInMeters);
            if (writeField[(int)FieldDescriptionId.MinimumDistanceAboveSurfaceInMeters]) WriteField(csvWriter, dwcObservation.Location.MinimumDistanceAboveSurfaceInMeters);
            if (writeField[(int)FieldDescriptionId.MinimumElevationInMeters]) WriteField(csvWriter, dwcObservation.Location.MinimumElevationInMeters);
            if (writeField[(int)FieldDescriptionId.Municipality]) WriteField(csvWriter, dwcObservation.Location.Municipality);
            if (writeField[(int)FieldDescriptionId.PointRadiusSpatialFit]) WriteField(csvWriter, dwcObservation.Location.PointRadiusSpatialFit);
            if (writeField[(int)FieldDescriptionId.StateProvince]) WriteField(csvWriter, dwcObservation.Location.StateProvince);
            if (writeField[(int)FieldDescriptionId.WaterBody]) WriteField(csvWriter, dwcObservation.Location.WaterBody);
            if (writeField[(int)FieldDescriptionId.VerbatimCoordinates]) WriteField(csvWriter, dwcObservation.Location.VerbatimCoordinates);
            if (writeField[(int)FieldDescriptionId.VerbatimCoordinateSystem]) WriteField(csvWriter, dwcObservation.Location.VerbatimCoordinateSystem);
            if (writeField[(int)FieldDescriptionId.VerbatimDepth]) WriteField(csvWriter, dwcObservation.Location.VerbatimDepth);
            if (writeField[(int)FieldDescriptionId.VerbatimElevation]) WriteField(csvWriter, dwcObservation.Location.VerbatimElevation);
            if (writeField[(int)FieldDescriptionId.VerbatimLatitude]) WriteField(csvWriter, dwcObservation.Location.VerbatimLatitude);
            if (writeField[(int)FieldDescriptionId.VerbatimLocality]) WriteField(csvWriter, dwcObservation.Location.VerbatimLocality);
            if (writeField[(int)FieldDescriptionId.VerbatimLongitude]) WriteField(csvWriter, dwcObservation.Location.VerbatimLongitude);
            if (writeField[(int)FieldDescriptionId.VerbatimSRS]) WriteField(csvWriter, dwcObservation.Location.VerbatimSRS);
            if (writeField[(int)FieldDescriptionId.Bed]) WriteField(csvWriter, dwcObservation.GeologicalContext?.Bed);
            if (writeField[(int)FieldDescriptionId.EarliestAgeOrLowestStage]) WriteField(csvWriter, dwcObservation.GeologicalContext?.EarliestAgeOrLowestStage);
            if (writeField[(int)FieldDescriptionId.EarliestEonOrLowestEonothem]) WriteField(csvWriter, dwcObservation.GeologicalContext?.EarliestEonOrLowestEonothem);
            if (writeField[(int)FieldDescriptionId.EarliestEpochOrLowestSeries]) WriteField(csvWriter, dwcObservation.GeologicalContext?.EarliestEpochOrLowestSeries);
            if (writeField[(int)FieldDescriptionId.EarliestEraOrLowestErathem]) WriteField(csvWriter, dwcObservation.GeologicalContext?.EarliestEraOrLowestErathem);
            if (writeField[(int)FieldDescriptionId.EarliestPeriodOrLowestSystem]) WriteField(csvWriter, dwcObservation.GeologicalContext?.EarliestPeriodOrLowestSystem);
            if (writeField[(int)FieldDescriptionId.Formation]) WriteField(csvWriter, dwcObservation.GeologicalContext?.Formation);
            if (writeField[(int)FieldDescriptionId.GeologicalContextID]) WriteField(csvWriter, dwcObservation.GeologicalContext?.GeologicalContextID);
            if (writeField[(int)FieldDescriptionId.Group]) WriteField(csvWriter, dwcObservation.GeologicalContext?.Group);
            if (writeField[(int)FieldDescriptionId.HighestBiostratigraphicZone]) WriteField(csvWriter, dwcObservation.GeologicalContext?.HighestBiostratigraphicZone);
            if (writeField[(int)FieldDescriptionId.LatestAgeOrHighestStage]) WriteField(csvWriter, dwcObservation.GeologicalContext?.LatestAgeOrHighestStage);
            if (writeField[(int)FieldDescriptionId.LatestEonOrHighestEonothem]) WriteField(csvWriter, dwcObservation.GeologicalContext?.LatestEonOrHighestEonothem);
            if (writeField[(int)FieldDescriptionId.LatestEpochOrHighestSeries]) WriteField(csvWriter, dwcObservation.GeologicalContext?.LatestEpochOrHighestSeries);
            if (writeField[(int)FieldDescriptionId.LatestEraOrHighestErathem]) WriteField(csvWriter, dwcObservation.GeologicalContext?.LatestEraOrHighestErathem);
            if (writeField[(int)FieldDescriptionId.LatestPeriodOrHighestSystem]) WriteField(csvWriter, dwcObservation.GeologicalContext?.LatestPeriodOrHighestSystem);
            if (writeField[(int)FieldDescriptionId.LithostratigraphicTerms]) WriteField(csvWriter, dwcObservation.GeologicalContext?.LithostratigraphicTerms);
            if (writeField[(int)FieldDescriptionId.LowestBiostratigraphicZone]) WriteField(csvWriter, dwcObservation.GeologicalContext?.LowestBiostratigraphicZone);
            if (writeField[(int)FieldDescriptionId.Member]) WriteField(csvWriter, dwcObservation.GeologicalContext?.Member);

            csvWriter.NextRecord();
        }


        private static void WriteField(CsvWriter csvWriter, string val, bool replaceNewLines = true)
        {
            if (val != null && replaceNewLines)
            {
                csvWriter.WriteField(val.RemoveNewLineTabs());
            }
            else
            {
                csvWriter.WriteField(val);
            }
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

        public void WriteHeaderRow(NReco.Csv.CsvWriter csvWriter, IEnumerable<FieldDescription> fieldDescriptions)
        {
            foreach (var fieldDescription in fieldDescriptions)
            {
                csvWriter.WriteField(fieldDescription.Name);
            }

            csvWriter.NextRecord();
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
                var csvWriter = new CsvWriter(streamWriter, "\t");

                // Write occurrence rows to CSV file.
                foreach (var dwcObservation in dwcObservations)
                {
                    WriteEventRow(csvWriter, dwcObservation, fieldsToWriteArray);
                }
                await streamWriter.FlushAsync();

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