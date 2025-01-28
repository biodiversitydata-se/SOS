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
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

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
        private const int FastSearchLimit = 10000;

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
            var temporaryZipExportFolderPath = Path.Combine(exportPath, "zip");

            try
            {                
                _fileService.CreateDirectory(temporaryZipExportFolderPath);
                var observationsFilePath = Path.Combine(temporaryZipExportFolderPath, $"{fileName}.csv");
                int expectedNoOfObservations = (int)await _processedObservationRepository.GetMatchCountAsync(filter);
                bool useFastSearch = expectedNoOfObservations <= FastSearchLimit;
                int nrObservations = 0;
                await using (var fileStream = File.Create(observationsFilePath))
                {
                    nrObservations = await WriteCsvFile(filter, culture, propertyLabelType, expectedNoOfObservations, useFastSearch, fileStream, cancellationToken);
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
                _logger.LogError(e, "Failed to create CSV file.");
                throw;
            }
            finally
            {
                _fileService.DeleteDirectory(temporaryZipExportFolderPath);
            }
        }        

        public async Task<byte[]> CreateFileInMemoryAsync(SearchFilter filter,            
            string culture,
            PropertyLabelType propertyLabelType,
            bool gzip,
            IJobCancellationToken cancellationToken)
        {            
            var memoryStream = new MemoryStream();

            try
            {
                int expectedNoOfObservations = (int)await _processedObservationRepository.GetMatchCountAsync(filter);
                bool useFastSearch = expectedNoOfObservations <= FastSearchLimit;
                int nrObservations = await WriteCsvFile(filter, culture, propertyLabelType, expectedNoOfObservations, useFastSearch, memoryStream, cancellationToken);                
                memoryStream.Seek(0, SeekOrigin.Begin);

                if (gzip)
                {
                    using var compressedStream = new MemoryStream();
                    using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal, true))
                    {
                        await memoryStream.CopyToAsync(gzipStream);
                    }                    
                    var result = compressedStream.ToArray();
                    await compressedStream.DisposeAsync();
                    return result;
                }
                else
                {
                    var result = memoryStream.ToArray();
                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create CSV file.");
                throw;
            }
            finally
            {
                if (memoryStream != null)
                    await memoryStream.DisposeAsync();
            }
        }

        public async Task<(Stream stream, string filename)> CreateFileInMemoryAsZipStreamAsync(SearchFilter filter,
           string culture,
           PropertyLabelType propertyLabelType,           
           IJobCancellationToken cancellationToken)
        {
            var memoryStream = new MemoryStream();

            try
            {
                int expectedNoOfObservations = (int)await _processedObservationRepository.GetMatchCountAsync(filter);
                bool useFastSearch = expectedNoOfObservations <= FastSearchLimit;
                string fileName = $"Observations {DateTime.Now.ToString("yyyy-MM-dd HH.mm")} SOS export";
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    var csvFileEntry = zipArchive.CreateEntry($"{fileName}.csv", useFastSearch ? CompressionLevel.Fastest : CompressionLevel.Optimal);
                    using (var csvFileZipStream = csvFileEntry.Open())
                    {
                        int nrObservations = await WriteCsvFile(filter, culture, propertyLabelType, expectedNoOfObservations, useFastSearch, csvFileZipStream, cancellationToken);
                    }                    

                    var jsonEntry = zipArchive.CreateEntry("filter.json");
                    using (var filterFileZipStream = jsonEntry.Open())
                    using (var writer = new StreamWriter(filterFileZipStream, Encoding.UTF8))
                    {
                        writer.Write(filter.GetFilterAsJson());
                    }
                }
                
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (memoryStream, fileName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create CSV file.");
                if (memoryStream != null) memoryStream.Dispose();
                throw;
            }            
        }

        private async Task<int> WriteCsvFile(
            SearchFilter filter, 
            string culture, 
            PropertyLabelType propertyLabelType, 
            int expectedNoOfObservations, 
            bool useFastSearch, 
            Stream stream, 
            IJobCancellationToken cancellationToken)
        {
            
            int nrObservations = 0;
            var propertyFields = ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(filter.Output?.Fields);
            using CsvFileHelper csvFileHelper = new CsvFileHelper();            
            csvFileHelper.InitializeWrite(stream, "\t", leaveStreamOpen: true);
            csvFileHelper.WriteRow(propertyFields.Select(pf => ObservationPropertyFieldDescriptionHelper.GetPropertyLabel(pf, propertyLabelType)));            
            Models.Search.Result.PagedResult<dynamic> fastSearchResult = null;
            Models.Search.Result.SearchAfterResult<Observation> searchResult = null;
            if (useFastSearch)
            {
                fastSearchResult = await _processedObservationRepository.GetChunkAsync(filter, 0, 10000);
            }
            else
            {
                searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter);
            }

            while ((useFastSearch && (fastSearchResult?.Records?.Any() ?? false)) || (!useFastSearch && (searchResult?.Records?.Any() ?? false)))
            {                
                cancellationToken?.ThrowIfCancellationRequested();
                Observation[] processedObservations = null;

                // Fetch observations from ElasticSearch.
                if (useFastSearch)
                {
                    processedObservations = fastSearchResult.Records.ToObservations().ToArray();
                }
                else
                {
                    processedObservations = searchResult.Records.ToArray();
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
                if (!useFastSearch)
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
            csvFileHelper.Flush();

            // If less tha 99% of expected observations where fetched, something is wrong
            if (nrObservations < expectedNoOfObservations * 0.99)
            {
                throw new Exception($"Csv export expected {expectedNoOfObservations} but only got {nrObservations}");
            }

            return nrObservations;
        }
    }
}
