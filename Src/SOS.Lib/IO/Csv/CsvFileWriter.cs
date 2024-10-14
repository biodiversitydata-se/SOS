using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IGeneralizationResolver _generalizationResolver;
        private readonly ILogger<CsvFileWriter> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="generalizationResolver"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CsvFileWriter(IProcessedObservationCoreRepository processedObservationRepository,
            IFileService fileService,
            IVocabularyValueResolver vocabularyValueResolver,
            IGeneralizationResolver generalizationResolver,
            ILogger<CsvFileWriter> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                                    throw new ArgumentNullException(
                                                        nameof(processedObservationRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _vocabularyValueResolver = vocabularyValueResolver ??
                                       throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _generalizationResolver = generalizationResolver ?? throw new ArgumentNullException(nameof(generalizationResolver));
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
            const int fastSearchLimit = 10000;
            var temporaryZipExportFolderPath = Path.Combine(exportPath, "zip");

            try
            {
                var nrObservations = 0;
                var propertyFields =
                    ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(filter.Output?.Fields);
                _fileService.CreateDirectory(temporaryZipExportFolderPath);

                var observationsFilePath = Path.Combine(temporaryZipExportFolderPath, $"{fileName}.csv");
                await using var fileStream = File.Create(observationsFilePath);
                using var csvFileHelper = new CsvFileHelper();
                csvFileHelper.InitializeWrite(fileStream, "\t");
                csvFileHelper.WriteRow(propertyFields.Select(pf => ObservationPropertyFieldDescriptionHelper.GetPropertyLabel(pf, propertyLabelType)));

                var expectedNoOfObservations = await _processedObservationRepository.GetMatchCountAsync(filter);
                bool usePointInTimeSearch = expectedNoOfObservations > fastSearchLimit;
                Models.Search.Result.PagedResult<dynamic> fastSearchResult = null;
                Models.Search.Result.SearchAfterResult<Observation> searchResult = null;                
                if (!usePointInTimeSearch)
                {
                    fastSearchResult = await _processedObservationRepository.GetChunkAsync(filter, 0, 10000);
                }
                else
                {
                    searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter);
                }

                while ((!usePointInTimeSearch && (fastSearchResult?.Records?.Any() ?? false)) || (usePointInTimeSearch && (searchResult?.Records?.Any() ?? false)))
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    Observation[] processedObservations = null;
                    // Fetch observations from ElasticSearch.
                    if (usePointInTimeSearch)
                    {
                        processedObservations = searchResult.Records.ToArray();
                    }
                    else
                    {
                        processedObservations = fastSearchResult.Records.ToObservations().ToArray();
                    }

                    // Resolve vocabulary values.
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, culture);
                    await _generalizationResolver.ResolveGeneralizedObservationsAsync(filter, processedObservations);

                    // Write occurrence rows to CSV file.
                    foreach (var observation in processedObservations)
                    {
                        var flatObservation = new FlatObservation(observation);
                        var fields = new List<string>();
                        foreach (var propertyField in propertyFields)
                        {
                            var value = flatObservation.GetValue(propertyField);
                            var stringValue = flatObservation.GetStringValue(propertyField);
                            fields.Add(stringValue);
                        }
                        csvFileHelper.WriteRow(fields);
                    }

                    nrObservations += processedObservations.Length;

                    // Get next batch of observations.
                    if (usePointInTimeSearch)
                    {
                        // Start fetching next batch of observations.
                        searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter, searchResult.PointInTimeId, searchResult.SearchAfter);
                    }
                    else
                    {
                        break;
                    }
                }

                csvFileHelper.FinishWrite();

                // If less tha 99% of expected observations where fetched, something is wrong
                if (nrObservations < expectedNoOfObservations * 0.99)
                {
                    throw new Exception($"Csv export expected {expectedNoOfObservations} but only got {nrObservations}");
                }

                if (gzip)
                {
                    await StoreFilterAsync(temporaryZipExportFolderPath, filter);
                    var zipFilePath = Path.Join(exportPath, $"{fileName}.zip");
                    _fileService.CompressDirectory(temporaryZipExportFolderPath, zipFilePath);
                    return new FileExportResult
                    {
                        NrObservations = nrObservations,
                        FilePath = zipFilePath
                    };
                }
                else
                {
                    var destinationFilePath = Path.Combine(exportPath, $"{fileName}.csv");
                    _fileService.MoveFile(observationsFilePath, destinationFilePath);
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
                _fileService.DeleteDirectory(temporaryZipExportFolderPath);
            }
        }
    }
}
