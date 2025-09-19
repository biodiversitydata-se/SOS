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
using Elastic.Clients.Elasticsearch;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Reports;
using SOS.Lib.Cache.Interfaces;

namespace SOS.Lib.IO.Excel
{
    /// <summary>
    /// Excel file writer.
    /// </summary>
    public class CsvFileWriter : FileWriterBase, ICsvFileWriter
    {
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly IFileService _fileService;
        private readonly IFilterManager _filterManager;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly IGeneralizationResolver _generalizationResolver;
        private readonly IAreaCache _areaCache;
        private readonly ICache<int, Taxon> _taxonCache;
        private readonly ILogger<CsvFileWriter> _logger;
        private const int FastSearchLimit = 10000;

        private static readonly string[] CountyReportHeaders = {"TaxonId", "ScientificName", "VernacularName", "OrganismLabel1", "OrganismLabel2", "SwedishOccurrence", "ImmigrationHistory",
                        "LastRedListCategory(2020)", "County", "CountyId", "CountyOccurrence", "ObservationCountAllDatasets", "ObservationCountArtportalen",
                        "ObservationCountArtportalenVerified", "ObservationCountAllDatasets(<2019)", "ObservationCountArtportalen(<2019)", "ObservationCountArtportalenVerified(<2019)",
                        "ObservationCountAllDatasets(>=2019)", "ObservationCountArtportalen(>=2019)", "ObservationCountArtportalenVerified(>=2019)", "LastRecorded"};

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
            Models.Search.Result.SearchAfterResult<Observation, ICollection<FieldValue>> searchResult = null;
            if (useFastSearch)
            {
                fastSearchResult = await _processedObservationRepository.GetChunkAsync<dynamic>(filter, 0, 10000);
            }
            else
            {
                searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter);
            }

            var fields = new List<string>(propertyFields.Count);

            while ((useFastSearch && (fastSearchResult?.Records?.Any() ?? false)) || (!useFastSearch && (searchResult?.Records?.Any() ?? false)))
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Observation[] processedObservations = null;

                // Fetch observations from ElasticSearch.
                if (useFastSearch)
                {
                    processedObservations = fastSearchResult.Records.ToObservationsArray();
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
                    fields.Clear();

                    foreach (var propertyField in propertyFields)
                    {
                        fields.Add(flatObservation.GetStringValue(propertyField));
                    }

                    csvFileHelper.WriteRow(fields);
                }

                nrObservations += processedObservations.Length;
                processedObservations = null;
                if (useFastSearch) break;

                // Get next batch of observations.                
                searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(
                    filter, searchResult.PointInTimeId, searchResult.SearchAfter);
            }

            searchResult = null;
            fastSearchResult = null;
            csvFileHelper.FinishWrite();
            await csvFileHelper.FlushAsync();
            if (nrObservations < expectedNoOfObservations * 0.99)
            {
                throw new Exception($"Csv export expected {expectedNoOfObservations} but only got {nrObservations}");
            }

            _logger.LogInformation(LogHelper.GetMemoryUsageSummary());
            return nrObservations;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="filterManager"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="generalizationResolver"></param>
        /// <param name="taxonCache"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CsvFileWriter(IProcessedObservationCoreRepository processedObservationRepository,
            IFileService fileService,
            IFilterManager filterManager,
            IVocabularyValueResolver vocabularyValueResolver,
            IGeneralizationResolver generalizationResolver,
            IAreaCache areaCache,
            ICache<int, Taxon> taxonCache,
            ILogger<CsvFileWriter> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                                    throw new ArgumentNullException(
                                                        nameof(processedObservationRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _vocabularyValueResolver = vocabularyValueResolver ??
                                       throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _generalizationResolver = generalizationResolver ?? throw new ArgumentNullException(nameof(generalizationResolver));
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
            _taxonCache = taxonCache ?? throw new ArgumentNullException(nameof(taxonCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

         /// <inheritdoc/>
        public async Task<FileExportResult> CreateCountyOccurrenceReportAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            string fileName,
            string reportPath,
            IEnumerable<int> taxonIds,
            IJobCancellationToken cancellationToken)
        {
            var temporaryZipExportFolderPath = Path.Combine(reportPath, "zip");
            const int maxCounties = 25;
            try
            {
                _fileService.CreateDirectory(temporaryZipExportFolderPath);
                var reportFilePath = Path.Combine(temporaryZipExportFolderPath, $"{fileName}.csv");
                var nrObservations = 0L;
                await using (var stream = File.Create(reportFilePath))
                {
                    using CsvFileHelper csvFileHelper = new CsvFileHelper();
                    csvFileHelper.InitializeWrite(stream, "\t", leaveStreamOpen: true);
                    csvFileHelper.WriteRow(CountyReportHeaders);
                    
                    const string aggregationField = "location.attributes.countyPartIdByCoordinate";
                    const string subAggregationField = "event.startDate";
                    foreach (var taxonId in taxonIds)
                    {
                        var filter = new SearchFilter
                        {
                            DataProviderIds = [],
                            Date = new DateFilter
                            {
                                StartDate = DateTime.MinValue,
                                EndDate = DateTime.Now,
                                DateFilterType = DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate
                            },
                            DiffusionStatuses = [DiffusionStatus.NotDiffused],
                            ExtendedAuthorization = new ExtendedAuthorizationFilter { ProtectionFilter = ProtectionFilter.BothPublicAndSensitive },
                            Taxa = new TaxonFilter { Ids = [taxonId], IncludeUnderlyingTaxa = true },
                            VerificationStatus = SearchFilterBase.StatusVerification.BothVerifiedAndNotVerified
                        };
                        await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
                        
                        var searchTasks = new Task<SearchResponse<dynamic>>[9];
                        searchTasks[0] = _processedObservationRepository.GenericTermsAggregationAsync(filter, aggregationField, maxCounties, subAggregationField, AggregationTypes.Max);
                        filter = filter.Clone();
                        filter.DataProviderIds = [1]; // Artportalen
                        searchTasks[1] = _processedObservationRepository.GenericTermsAggregationAsync(filter, aggregationField, maxCounties);
                        filter = filter.Clone();
                        filter.VerificationStatus = SearchFilterBase.StatusVerification.Verified;
                        searchTasks[2] = _processedObservationRepository.GenericTermsAggregationAsync(filter, aggregationField, maxCounties);
                        filter = filter.Clone();
                        filter.DataProviderIds = [];
                        filter.Date.StartDate = DateTime.MinValue;
                        filter.Date.EndDate = new DateTime(2018, 12, 31, 23, 59, 59);
                        filter.VerificationStatus = SearchFilterBase.StatusVerification.BothVerifiedAndNotVerified;
                        searchTasks[3] = _processedObservationRepository.GenericTermsAggregationAsync(filter, aggregationField, maxCounties);
                        filter = filter.Clone();
                        filter.DataProviderIds = [1];
                        searchTasks[4] = _processedObservationRepository.GenericTermsAggregationAsync(filter, aggregationField, maxCounties);
                        filter = filter.Clone();
                        filter.VerificationStatus = SearchFilterBase.StatusVerification.Verified;
                        searchTasks[5] = _processedObservationRepository.GenericTermsAggregationAsync(filter, aggregationField, maxCounties);
                        filter = filter.Clone();
                        filter.DataProviderIds = [];
                        filter.Date.StartDate = new DateTime(2019, 01, 01);
                        filter.Date.EndDate = DateTime.Now;
                        filter.VerificationStatus = SearchFilterBase.StatusVerification.BothVerifiedAndNotVerified;
                        searchTasks[6] = _processedObservationRepository.GenericTermsAggregationAsync(filter, aggregationField, maxCounties);
                        filter = filter.Clone();
                        filter.DataProviderIds = [1];
                        searchTasks[7] = _processedObservationRepository.GenericTermsAggregationAsync(filter, aggregationField, maxCounties);
                        filter = filter.Clone();
                        filter.VerificationStatus = SearchFilterBase.StatusVerification.Verified;
                        searchTasks[8] = _processedObservationRepository.GenericTermsAggregationAsync(filter, aggregationField, maxCounties);
                        await Task.WhenAll(searchTasks);

                        var taxonData = new Dictionary<long, CountyOccurrenceReportRow>();
                        for (var i = 0; i < 9; i++)
                        {
                            var searchTask = searchTasks[i];
                            var countiesData = searchTask.Result
                                .Aggregations
                                    .GetStringTerms(aggregationField)
                                        .Buckets?
                                            .Select(b =>
                                                new
                                                {
                                                    Count = b.DocCount,
                                                    Key = b.Key.Value,
                                                    LastFound = b.Aggregations?.GetMax("max")?.ValueAsString
                                                }
                                            )?.ToArray();
                            foreach(var countyData in countiesData)
                            {
                                int.TryParse(countyData.Key?.ToString(), out var countyId);
                                countyId = Math.Abs(countyId);
                                if (!taxonData.TryGetValue(countyId, out var reportRow))
                                {
                                    var countyName = countyId switch
                                    {
                                        100 => "Kalmar (fastland)", // Special case Kalmar has been separated from Öland
                                        101 => "Öland",
                                        _ => (await _areaCache.GetAsync(AreaType.County, countyData.Key.ToString()))?.Name
                                    };
                                    reportRow = new CountyOccurrenceReportRow
                                    {
                                        County = countyName,
                                        CountyId = countyId
                                    };
                                    taxonData.Add(countyId, reportRow);
                                }
                                
                                switch (i)
                                {
                                    case 0:
                                        reportRow.ObservationCountAllDatasets += countyData.Count;
                                        if (DateTime.TryParse(countyData.LastFound, out var lastRecorded))
                                        {
                                            if (!reportRow.LastRecorded.HasValue || reportRow.LastRecorded < lastRecorded)
                                            {
                                                reportRow.LastRecorded = lastRecorded;
                                            }
                                        }
                                        nrObservations += countyData.Count;
                                        break;
                                    case 1:
                                        reportRow.ObservationCountArtportalen += countyData.Count;
                                        break;
                                    case 2:
                                        reportRow.ObservationCountArtportalenVerified += countyData.Count;
                                        break;
                                    case 3:
                                        reportRow.ObservationCountAllDatasetsPre2019 += countyData.Count;
                                        break;
                                    case 4:
                                        reportRow.ObservationCountArtportalenPre2019 += countyData.Count;
                                        break;
                                    case 5:
                                        reportRow.ObservationCountArtportalenVerifiedPre2019 += countyData.Count;
                                        break;
                                    case 6:
                                        reportRow.ObservationCountAllDatasetsPost2018 += countyData.Count;
                                        break;
                                    case 7:
                                        reportRow.ObservationCountArtportalenPost2018 += countyData.Count;
                                        break;
                                    case 8:
                                        reportRow.ObservationCountArtportalenVerifiedPost2018 += countyData.Count;
                                        break;
                                }
                            }
                        }
                        var taxon = (await _taxonCache.GetAsync(taxonId)) ?? new Taxon
                        {
                            Id = taxonId,
                            ScientificName = "Missing taxon"
                        };
                        var taxonCountryOccurrences = taxon.Attributes?.CountyOccurrences?.ToDictionary(co => co.Id, co => co) ?? new Dictionary<int, Models.DarwinCore.CountyOccurrence>();
                        foreach (var reportRow in taxonData.Values)
                        {
                            taxonCountryOccurrences.TryGetValue(reportRow.CountyId, out var countyOccurrence);
                              
                            csvFileHelper.WriteRow([
                                taxon.Id.ToString(),
                                taxon.ScientificName,
                                taxon.VernacularName,
                                taxon.Attributes?.OrganismLabel1,
                                taxon.Attributes?.OrganismLabel2,
                                taxon.Attributes?.SwedishOccurrence,
                                taxon.Attributes?.SwedishHistory,
                                taxon.Attributes?.RedlistCategory,
                                reportRow.County,
                                reportRow.CountyId.ToString(),
                                countyOccurrence?.Status ?? "saknas",
                                reportRow.ObservationCountAllDatasets.ToString(),
                                reportRow.ObservationCountArtportalen.ToString(),
                                reportRow.ObservationCountArtportalenVerified.ToString(),
                                reportRow.ObservationCountAllDatasetsPre2019.ToString(),
                                reportRow.ObservationCountArtportalenPre2019.ToString(),
                                reportRow.ObservationCountArtportalenVerifiedPre2019.ToString(),
                                reportRow.ObservationCountAllDatasetsPost2018.ToString(),
                                reportRow.ObservationCountArtportalenPost2018.ToString(),
                                reportRow.ObservationCountArtportalenVerifiedPost2018.ToString(),
                                reportRow.LastRecorded?.ToString("yyyy-MM-dd")
                           ]);
                        }
                        await Task.Delay(2500); // sleep 2,5 sec to give Elastic some rest
                    }
                    csvFileHelper.FinishWrite();
                    await csvFileHelper.FlushAsync();
                   _logger.LogInformation(LogHelper.GetMemoryUsageSummary());
                }

                var zipFilePath = Path.Join(reportPath, $"{fileName}.zip");
                _fileService.CompressDirectory(temporaryZipExportFolderPath, zipFilePath);
                return new FileExportResult
                {
                    NrObservations = (int)nrObservations,
                    FilePath = zipFilePath
                };
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
    }
}