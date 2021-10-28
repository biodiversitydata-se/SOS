﻿using System;
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
            if (writeField[(int)FieldDescriptionId.EventID]) WriteField(csvFileHelper, dwcObservation.Event.EventID);
            if (writeField[(int)FieldDescriptionId.ParentEventID]) WriteField(csvFileHelper, dwcObservation.Event.ParentEventID);
            if (writeField[(int)FieldDescriptionId.EventDate]) WriteField(csvFileHelper, dwcObservation.Event.EventDate);
            if (writeField[(int)FieldDescriptionId.VerbatimEventDate]) WriteField(csvFileHelper, dwcObservation.Event.VerbatimEventDate);
            if (writeField[(int)FieldDescriptionId.EventTime]) WriteField(csvFileHelper, dwcObservation.Event.EventTime);
            if (writeField[(int)FieldDescriptionId.EventRemarks]) WriteField(csvFileHelper, dwcObservation.Event.EventRemarks);
            if (writeField[(int)FieldDescriptionId.FieldNotes]) WriteField(csvFileHelper, dwcObservation.Event.FieldNotes);
            if (writeField[(int)FieldDescriptionId.FieldNumber]) WriteField(csvFileHelper, dwcObservation.Event.FieldNumber);
            if (writeField[(int)FieldDescriptionId.Habitat]) WriteField(csvFileHelper, dwcObservation.Event.Habitat);
            if (writeField[(int)FieldDescriptionId.SampleSizeValue]) WriteField(csvFileHelper, dwcObservation.Event.SampleSizeValue);
            if (writeField[(int)FieldDescriptionId.SampleSizeUnit]) WriteField(csvFileHelper, dwcObservation.Event.SampleSizeUnit);
            if (writeField[(int)FieldDescriptionId.SamplingEffort]) WriteField(csvFileHelper, dwcObservation.Event.SamplingEffort);
            if (writeField[(int)FieldDescriptionId.SamplingProtocol]) WriteField(csvFileHelper, dwcObservation.Event.SamplingProtocol);
            if (writeField[(int)FieldDescriptionId.Day]) WriteField(csvFileHelper, dwcObservation.Event.Day.HasValue ? dwcObservation.Event.Day.ToString() : null);
            if (writeField[(int)FieldDescriptionId.Month]) WriteField(csvFileHelper, dwcObservation.Event.Month.HasValue ? dwcObservation.Event.Month.ToString() : null);
            if (writeField[(int)FieldDescriptionId.Year]) WriteField(csvFileHelper, dwcObservation.Event.Year.HasValue ? dwcObservation.Event.Year.ToString() : null);
            if (writeField[(int)FieldDescriptionId.EndDayOfYear]) WriteField(csvFileHelper, dwcObservation.Event.EndDayOfYear.HasValue ? dwcObservation.Event.EndDayOfYear.ToString() : null);
            if (writeField[(int)FieldDescriptionId.StartDayOfYear]) WriteField(csvFileHelper, dwcObservation.Event.StartDayOfYear.HasValue ? dwcObservation.Event.StartDayOfYear.ToString() : null);
            if (writeField[(int)FieldDescriptionId.BasisOfRecord]) WriteField(csvFileHelper, dwcObservation.BasisOfRecord);
            if (writeField[(int)FieldDescriptionId.BibliographicCitation]) WriteField(csvFileHelper, dwcObservation.BibliographicCitation);
            if (writeField[(int)FieldDescriptionId.CollectionCode]) WriteField(csvFileHelper, dwcObservation.CollectionCode);
            if (writeField[(int)FieldDescriptionId.CollectionID]) WriteField(csvFileHelper, dwcObservation.CollectionID);
            if (writeField[(int)FieldDescriptionId.DataGeneralizations]) WriteField(csvFileHelper, dwcObservation.DataGeneralizations);
            if (writeField[(int)FieldDescriptionId.DatasetID]) WriteField(csvFileHelper, dwcObservation.DatasetID);
            if (writeField[(int)FieldDescriptionId.DatasetName]) WriteField(csvFileHelper, dwcObservation.DatasetName);
            if (writeField[(int)FieldDescriptionId.DynamicProperties]) WriteField(csvFileHelper, dwcObservation.DynamicProperties);
            if (writeField[(int)FieldDescriptionId.InformationWithheld]) WriteField(csvFileHelper, dwcObservation.InformationWithheld);
            if (writeField[(int)FieldDescriptionId.InstitutionCode]) WriteField(csvFileHelper, dwcObservation.InstitutionCode);
            if (writeField[(int)FieldDescriptionId.InstitutionID]) WriteField(csvFileHelper, dwcObservation.InstitutionID);
            if (writeField[(int)FieldDescriptionId.Language]) WriteField(csvFileHelper, dwcObservation.Language);
            if (writeField[(int)FieldDescriptionId.License]) WriteField(csvFileHelper, dwcObservation.License);
            if (writeField[(int)FieldDescriptionId.Modified]) WriteField(csvFileHelper, dwcObservation.Modified?.ToString("s", CultureInfo.InvariantCulture));
            if (writeField[(int)FieldDescriptionId.OwnerInstitutionCode]) WriteField(csvFileHelper, dwcObservation.OwnerInstitutionCode);
            if (writeField[(int)FieldDescriptionId.References]) WriteField(csvFileHelper, dwcObservation.References);
            if (writeField[(int)FieldDescriptionId.RightsHolder]) WriteField(csvFileHelper, dwcObservation.RightsHolder);
            if (writeField[(int)FieldDescriptionId.Type]) WriteField(csvFileHelper, dwcObservation.Type);
            if (writeField[(int)FieldDescriptionId.CoordinatePrecision]) WriteField(csvFileHelper, dwcObservation.Location.CoordinatePrecision);
            if (writeField[(int)FieldDescriptionId.CoordinateUncertaintyInMeters]) WriteField(csvFileHelper, GetCoordinateUncertaintyInMetersValue(dwcObservation.Location.CoordinateUncertaintyInMeters));
            if (writeField[(int)FieldDescriptionId.Country]) WriteField(csvFileHelper, dwcObservation.Location.Country);
            if (writeField[(int)FieldDescriptionId.CountryCode]) WriteField(csvFileHelper, dwcObservation.Location.CountryCode);
            if (writeField[(int)FieldDescriptionId.County]) WriteField(csvFileHelper, dwcObservation.Location.County);
            if (writeField[(int)FieldDescriptionId.DecimalLatitude]) WriteField(csvFileHelper, dwcObservation.Location.DecimalLatitude?.ToString("F5", CultureInfo.InvariantCulture));
            if (writeField[(int)FieldDescriptionId.DecimalLongitude]) WriteField(csvFileHelper, dwcObservation.Location.DecimalLongitude?.ToString("F5", CultureInfo.InvariantCulture));
            if (writeField[(int)FieldDescriptionId.FootprintSpatialFit]) WriteField(csvFileHelper, dwcObservation.Location.FootprintSpatialFit);
            if (writeField[(int)FieldDescriptionId.FootprintSRS]) WriteField(csvFileHelper, dwcObservation.Location.FootprintSRS);
            if (writeField[(int)FieldDescriptionId.FootprintWKT]) WriteField(csvFileHelper, dwcObservation.Location.FootprintWKT);
            if (writeField[(int)FieldDescriptionId.GeodeticDatum]) WriteField(csvFileHelper, dwcObservation.Location.GeodeticDatum);
            if (writeField[(int)FieldDescriptionId.GeoreferencedBy]) WriteField(csvFileHelper, dwcObservation.Location.GeoreferencedBy);
            if (writeField[(int)FieldDescriptionId.GeoreferencedDate]) WriteField(csvFileHelper, dwcObservation.Location.GeoreferencedDate);
            if (writeField[(int)FieldDescriptionId.GeoreferenceProtocol]) WriteField(csvFileHelper, dwcObservation.Location.GeoreferenceProtocol);
            if (writeField[(int)FieldDescriptionId.GeoreferenceRemarks]) WriteField(csvFileHelper, dwcObservation.Location.GeoreferenceRemarks);
            if (writeField[(int)FieldDescriptionId.GeoreferenceSources]) WriteField(csvFileHelper, dwcObservation.Location.GeoreferenceSources);
            if (writeField[(int)FieldDescriptionId.GeoreferenceVerificationStatus]) WriteField(csvFileHelper, dwcObservation.Location.GeoreferenceVerificationStatus);
            if (writeField[(int)FieldDescriptionId.HigherGeography]) WriteField(csvFileHelper, dwcObservation.Location.HigherGeography);
            if (writeField[(int)FieldDescriptionId.HigherGeographyID]) WriteField(csvFileHelper, dwcObservation.Location.HigherGeographyID);
            if (writeField[(int)FieldDescriptionId.Continent]) WriteField(csvFileHelper, dwcObservation.Location.Continent);
            if (writeField[(int)FieldDescriptionId.Island]) WriteField(csvFileHelper, dwcObservation.Location.Island);
            if (writeField[(int)FieldDescriptionId.IslandGroup]) WriteField(csvFileHelper, dwcObservation.Location.IslandGroup);
            if (writeField[(int)FieldDescriptionId.Locality]) WriteField(csvFileHelper, dwcObservation.Location.Locality);
            if (writeField[(int)FieldDescriptionId.LocationAccordingTo]) WriteField(csvFileHelper, dwcObservation.Location.LocationAccordingTo);
            if (writeField[(int)FieldDescriptionId.LocationID]) WriteField(csvFileHelper, dwcObservation.Location.LocationID);
            if (writeField[(int)FieldDescriptionId.LocationRemarks]) WriteField(csvFileHelper, dwcObservation.Location.LocationRemarks);
            if (writeField[(int)FieldDescriptionId.MaximumDepthInMeters]) WriteField(csvFileHelper, dwcObservation.Location.MaximumDepthInMeters);
            if (writeField[(int)FieldDescriptionId.MaximumDistanceAboveSurfaceInMeters]) WriteField(csvFileHelper, dwcObservation.Location.MaximumDistanceAboveSurfaceInMeters);
            if (writeField[(int)FieldDescriptionId.MaximumElevationInMeters]) WriteField(csvFileHelper, dwcObservation.Location.MaximumElevationInMeters);
            if (writeField[(int)FieldDescriptionId.MinimumDepthInMeters]) WriteField(csvFileHelper, dwcObservation.Location.MinimumDepthInMeters);
            if (writeField[(int)FieldDescriptionId.MinimumDistanceAboveSurfaceInMeters]) WriteField(csvFileHelper, dwcObservation.Location.MinimumDistanceAboveSurfaceInMeters);
            if (writeField[(int)FieldDescriptionId.MinimumElevationInMeters]) WriteField(csvFileHelper, dwcObservation.Location.MinimumElevationInMeters);
            if (writeField[(int)FieldDescriptionId.Municipality]) WriteField(csvFileHelper, dwcObservation.Location.Municipality);
            if (writeField[(int)FieldDescriptionId.PointRadiusSpatialFit]) WriteField(csvFileHelper, dwcObservation.Location.PointRadiusSpatialFit);
            if (writeField[(int)FieldDescriptionId.StateProvince]) WriteField(csvFileHelper, dwcObservation.Location.StateProvince);
            if (writeField[(int)FieldDescriptionId.WaterBody]) WriteField(csvFileHelper, dwcObservation.Location.WaterBody);
            if (writeField[(int)FieldDescriptionId.VerbatimCoordinates]) WriteField(csvFileHelper, dwcObservation.Location.VerbatimCoordinates);
            if (writeField[(int)FieldDescriptionId.VerbatimCoordinateSystem]) WriteField(csvFileHelper, dwcObservation.Location.VerbatimCoordinateSystem);
            if (writeField[(int)FieldDescriptionId.VerbatimDepth]) WriteField(csvFileHelper, dwcObservation.Location.VerbatimDepth);
            if (writeField[(int)FieldDescriptionId.VerbatimElevation]) WriteField(csvFileHelper, dwcObservation.Location.VerbatimElevation);
            if (writeField[(int)FieldDescriptionId.VerbatimLatitude]) WriteField(csvFileHelper, dwcObservation.Location.VerbatimLatitude);
            if (writeField[(int)FieldDescriptionId.VerbatimLocality]) WriteField(csvFileHelper, dwcObservation.Location.VerbatimLocality);
            if (writeField[(int)FieldDescriptionId.VerbatimLongitude]) WriteField(csvFileHelper, dwcObservation.Location.VerbatimLongitude);
            if (writeField[(int)FieldDescriptionId.VerbatimSRS]) WriteField(csvFileHelper, dwcObservation.Location.VerbatimSRS);
            if (writeField[(int)FieldDescriptionId.Bed]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.Bed);
            if (writeField[(int)FieldDescriptionId.EarliestAgeOrLowestStage]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.EarliestAgeOrLowestStage);
            if (writeField[(int)FieldDescriptionId.EarliestEonOrLowestEonothem]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.EarliestEonOrLowestEonothem);
            if (writeField[(int)FieldDescriptionId.EarliestEpochOrLowestSeries]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.EarliestEpochOrLowestSeries);
            if (writeField[(int)FieldDescriptionId.EarliestEraOrLowestErathem]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.EarliestEraOrLowestErathem);
            if (writeField[(int)FieldDescriptionId.EarliestPeriodOrLowestSystem]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.EarliestPeriodOrLowestSystem);
            if (writeField[(int)FieldDescriptionId.Formation]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.Formation);
            if (writeField[(int)FieldDescriptionId.GeologicalContextID]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.GeologicalContextID);
            if (writeField[(int)FieldDescriptionId.Group]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.Group);
            if (writeField[(int)FieldDescriptionId.HighestBiostratigraphicZone]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.HighestBiostratigraphicZone);
            if (writeField[(int)FieldDescriptionId.LatestAgeOrHighestStage]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.LatestAgeOrHighestStage);
            if (writeField[(int)FieldDescriptionId.LatestEonOrHighestEonothem]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.LatestEonOrHighestEonothem);
            if (writeField[(int)FieldDescriptionId.LatestEpochOrHighestSeries]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.LatestEpochOrHighestSeries);
            if (writeField[(int)FieldDescriptionId.LatestEraOrHighestErathem]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.LatestEraOrHighestErathem);
            if (writeField[(int)FieldDescriptionId.LatestPeriodOrHighestSystem]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.LatestPeriodOrHighestSystem);
            if (writeField[(int)FieldDescriptionId.LithostratigraphicTerms]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.LithostratigraphicTerms);
            if (writeField[(int)FieldDescriptionId.LowestBiostratigraphicZone]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.LowestBiostratigraphicZone);
            if (writeField[(int)FieldDescriptionId.Member]) WriteField(csvFileHelper, dwcObservation.GeologicalContext?.Member);

            csvFileHelper.NextRecord();
        }


        private static void WriteField(CsvFileHelper csvFileHelper, string val, bool replaceNewLines = true)
        {
            if (val != null && replaceNewLines)
            {
                csvFileHelper.WriteField(val.RemoveNewLineTabs());
            }
            else
            {
                csvFileHelper.WriteField(val);
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