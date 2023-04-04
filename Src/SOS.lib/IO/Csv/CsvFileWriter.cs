﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace SOS.Lib.IO.Excel
{
    /// <summary>
    /// Excel file writer.
    /// </summary>
    public class CsvFileWriter : FileWriterBase, ICsvFileWriter
    {
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly IFileService _fileService;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly ILogger<CsvFileWriter> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="telemetry"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CsvFileWriter(IProcessedObservationCoreRepository processedObservationRepository, 
            IFileService fileService,
            IVocabularyValueResolver vocabularyValueResolver,
            TelemetryClient telemetry,
            ILogger<CsvFileWriter> logger) : base(telemetry)
        {
            _processedObservationRepository = processedObservationRepository ??
                                                    throw new ArgumentNullException(
                                                        nameof(processedObservationRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _vocabularyValueResolver = vocabularyValueResolver ??
                                       throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FileExportResult> CreateFileAync(SearchFilter filter, 
            string exportPath,
            string fileName, 
            string culture,
            PropertyLabelType propertyLabelType,
            bool gzip,
            IJobCancellationToken cancellationToken)
        {
            string temporaryZipExportFolderPath = null;
            
            try
            {
                using var operation = _telemetry.StartOperation<DependencyTelemetry>("Create_Csv-File");
                operation.Telemetry.Properties["Filter"] = filter.ToString();

                var nrObservations = 0;
                var propertyFields =
                    ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(filter.Output?.Fields);
                temporaryZipExportFolderPath = Path.Combine(exportPath, fileName);
                if (!Directory.Exists(temporaryZipExportFolderPath))
                {
                    Directory.CreateDirectory(temporaryZipExportFolderPath);
                }

                var observationsFilePath = Path.Combine(temporaryZipExportFolderPath, "Observations.csv");
                await using var fileStream = File.Create(observationsFilePath);                
                using var csvFileHelper = new CsvFileHelper();
                csvFileHelper.InitializeWrite(fileStream, "\t");
                csvFileHelper.WriteRow(propertyFields.Select(pf => ObservationPropertyFieldDescriptionHelper.GetPropertyLabel(pf, propertyLabelType)));

                var expectedNoOfObservations = await _processedObservationRepository.GetMatchCountAsync(filter);
                var searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter);

                while (searchResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    // Start fetching next batch of observations.
                    var searchResultTask = _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter, searchResult.PointInTimeId, searchResult.SearchAfter);
                    
                    // Fetch observations from ElasticSearch.
                    var processedObservations = searchResult.Records.ToArray();

                    // Resolve vocabulary values.
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, culture);

                    // Write occurrence rows to CSV file.
                    foreach (var observation in processedObservations)
                    {
                        var flatObservation = new FlatObservation(observation);
                        var fields = new List<string>();
                        foreach (var propertyField in propertyFields)
                        {
                            var value = flatObservation.GetValue(propertyField);
                            var stringValue = value == null ? string.Empty : propertyField.DataTypeEnum switch
                            {
                                PropertyFieldDataType.Boolean => ((bool?)value)?.ToString(CultureInfo.InvariantCulture),
                                PropertyFieldDataType.DateTime => ((DateTime?) value)?.ToShortDateString(),
                                PropertyFieldDataType.Double => ((double) value).ToString(CultureInfo.InvariantCulture),
                                PropertyFieldDataType.Int32 => ((int)value).ToString(),
                                PropertyFieldDataType.Int64 => ((long)value).ToString(),
                                PropertyFieldDataType.TimeSpan => ((TimeSpan?)value)?.ToString("hh\\:mm"),
                                _ => (string)value
                            };
                            fields.Add(stringValue);
                        }
                        csvFileHelper.WriteRow(fields);
                    }
                 
                    nrObservations += processedObservations.Length;
                    
                    // Get next batch of observations.
                    searchResult = await searchResultTask;
                }

                csvFileHelper.FinishWrite();

                // If less tha 99% of expected observations where fetched, something is wrong
                if (nrObservations < expectedNoOfObservations * 0.99)
                {
                    throw new Exception($"Csv export expected {expectedNoOfObservations} but only got {nrObservations}");
                }

                operation.Telemetry.Metrics["Observation-count"] = nrObservations;

                if (gzip)
                {
                    await StoreFilterAsync(temporaryZipExportFolderPath, filter);
                    var zipFilePath = _fileService.CompressFolder(exportPath, fileName);
                    return new FileExportResult
                    {
                        NrObservations = nrObservations,
                        FilePath = zipFilePath
                    };
                }
                else
                {
                    var destinationFilePath = Path.Combine(exportPath, $"{fileName}.csv");
                    File.Move(observationsFilePath, destinationFilePath);
                    return new FileExportResult
                    {
                        NrObservations = nrObservations,
                        FilePath = destinationFilePath
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Excel File.");
                throw;
            }
            finally
            {
                _fileService.DeleteFolder(temporaryZipExportFolderPath);
            }
        }
    }
}
