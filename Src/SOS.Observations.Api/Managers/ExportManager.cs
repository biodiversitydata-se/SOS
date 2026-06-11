using Hangfire;
using Hangfire.Server;
using SOS.Lib.Dtos.Enum;
using SOS.Lib.Dtos.Filter;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.IO.Csv.Interfaces;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Managers;

/// <summary>
/// Manager for exports
/// </summary>
public class ExportManager : IExportManager
{
    private readonly IFilterManager _filterManager;
    private readonly IFilterHelper _filterHelper;
    private readonly IDwcArchiveFileWriter _dwcArchiveFileWriter;
    private readonly IDwcArchiveEventFileWriter _dwcArchiveEventFileWriter;
    private readonly ICsvFileWriter _csvWriter;
    private readonly IExcelFileWriter _excelWriter;
    private readonly IGeoJsonFileWriter _geoJsonWriter;
    private readonly ILogger<ExportManager> _logger;
    private readonly IProcessedObservationRepository _processedObservationRepository;
    private readonly IProcessInfoRepository _processInfoRepository;
    private static readonly SemaphoreSlim _downloadSemaphore = new SemaphoreSlim(3, 3);


    private async Task<(Stream stream, string filename)> CreateDwcExportInMemoryAsync(
        SearchFilter filter,
        IEnumerable<(Stream Stream, string Filename)> additionalFiles,
        bool eventDwC,
        IJobCancellationToken cancellationToken)
    {
        try
        {
            var processInfo = await _processInfoRepository.GetAsync(_processedObservationRepository.PublicIndexName);

            if (eventDwC)
            {
                filter.Output.SortOrders = new[] { new SortOrderFilter { SortBy = "event.eventId", SortOrder = SearchSortOrder.Asc } };
                return await _dwcArchiveEventFileWriter.CreateEventDwcArchiveFileInMemoryAsync(
                   DataProvider.FilterSubsetDataProvider,
                   filter,
                   additionalFiles,
                   _processedObservationRepository,
                   processInfo,
                   cancellationToken);
            }
            else
            {
                var propertyFields =
                   ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(filter?.Output?.Fields);
                var fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions().
                   Where(fd => propertyFields.Select(pf => pf?.DwcIdentifier?.ToLower()).Contains(fd?.DwcIdentifier?.ToLower()));

                return await _dwcArchiveFileWriter.CreateDwcArchiveFileInMemoryAsync(
                    DataProvider.FilterSubsetDataProvider,
                    filter,
                    additionalFiles,
                    _processedObservationRepository,
                    fieldDescriptions,
                    processInfo,
                    cancellationToken);
            }
        }
        catch (JobAbortedException)
        {
            _logger.LogInformation("Export sightings was canceled.");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to export sightings");
            throw;
        }
    }

    
    /// <summary>
    /// Initializes a new instance of the ExportManager class, which coordinates the export of observation data to
    /// various file formats and manages related export operations.
    /// </summary>
    /// <param name="csvWriter">The CSV file writer used to generate CSV exports. Cannot be null.</param>
    /// <param name="dwcArchiveFileWriter">The writer responsible for creating Darwin Core Archive files. Cannot be null.</param>
    /// <param name="dwcArchiveEventFileWriter">The writer used to generate Darwin Core Archive event files. Cannot be null.</param>
    /// <param name="excelWriter">The Excel file writer used to create Excel exports. Cannot be null.</param>
    /// <param name="geoJsonWriter">The GeoJSON file writer used to generate GeoJSON exports. Cannot be null.</param>
    /// <param name="processedObservationRepository">The repository for accessing processed observation data. Cannot be null.</param>
    /// <param name="processInfoRepository">The repository for accessing process information related to exports. Cannot be null.</param>
    /// <param name="filterManager">The filter manager used to apply and manage export filters. Cannot be null.</param>
    /// <param name="filterHelper">The filter helper used to assist with filter operations during export. Cannot be null.</param>
    /// <param name="logger">The logger used for logging export operations and errors. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public ExportManager(
        ICsvFileWriter csvWriter,
        IDwcArchiveFileWriter dwcArchiveFileWriter,
        IDwcArchiveEventFileWriter dwcArchiveEventFileWriter,
        IExcelFileWriter excelWriter,
        IGeoJsonFileWriter geoJsonWriter,
        IProcessedObservationRepository processedObservationRepository,
        IProcessInfoRepository processInfoRepository,
        IFilterManager filterManager,
        IFilterHelper filterHelper,
        ILogger<ExportManager> logger)
    {
        _csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
        _dwcArchiveFileWriter =
            dwcArchiveFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriter));
        _dwcArchiveEventFileWriter =
            dwcArchiveEventFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveEventFileWriter));
        _excelWriter =
            excelWriter ?? throw new ArgumentNullException(nameof(excelWriter));
        _geoJsonWriter =
            geoJsonWriter ?? throw new ArgumentNullException(nameof(geoJsonWriter));

        _processedObservationRepository = processedObservationRepository ??
                                          throw new ArgumentNullException(nameof(processedObservationRepository));
        _processInfoRepository =
            processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
        _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
        _filterHelper = filterHelper ?? throw new ArgumentNullException(nameof(filterHelper));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<(Stream stream, string filename)> CreateExportFileInMemoryAsZipStreamAsync(
        int? userId,
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilterBaseDto searchFilter,
        ProtectionFilterDto? protectionFilter,
        ExportFormat exportFormat,
        string culture,
        bool flatOut,
        PropertyLabelType propertyLabelType,
        bool excludeNullValues,
        bool dynamicProjectDataFields,
        bool gzip,
        IJobCancellationToken cancellationToken)
    {   
        if (!await _downloadSemaphore.WaitAsync(TimeSpan.FromSeconds(60)))
        {
            throw new SemaphoreTimeoutException();
        }

        try
        {
            var filter = searchFilter.ToSearchFilter(userId ?? 0, protectionFilter, culture ?? "en-GB");
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            var additionalFiles = new List<(Stream stream, string filename)>();
            var filterMarkdownStream = await _filterHelper.GetMarkDownAsync(searchFilter, filter, CancellationToken.None);
            additionalFiles.Add((filterMarkdownStream, "filter.md"));
            if ((searchFilter?.Geographics?.Geometries?.Count() ?? 0) != 0)
            {
                var geoJsonStream = _filterHelper.TryGetFeatureCollection(searchFilter.Geographics.Geometries);
                if (geoJsonStream != null)
                {
                    additionalFiles.Add((geoJsonStream, "geometries.geojson"));
                }
            }
            var fileExportResult = exportFormat switch
            {
                ExportFormat.Csv => await _csvWriter.CreateFileInMemoryAsZipStreamAsync(filter, additionalFiles, culture, propertyLabelType, cancellationToken),
                ExportFormat.DwCEvent => await CreateDwcExportInMemoryAsync(filter, additionalFiles, eventDwC: true, cancellationToken),
                ExportFormat.Excel => await _excelWriter.CreateFileInMemoryAsync(filter, additionalFiles, culture, propertyLabelType, dynamicProjectDataFields, gzip, cancellationToken),
                ExportFormat.GeoJson => await _geoJsonWriter.CreateFileInMemoryAsZipStreamAsync(filter, additionalFiles, culture, flatOut, propertyLabelType, excludeNullValues, cancellationToken),
                ExportFormat.DwC => await CreateDwcExportInMemoryAsync(filter, additionalFiles, eventDwC: false, cancellationToken),
                _ => throw new NotImplementedException()
            };

            return fileExportResult;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create export file");
            throw;
        }
        finally
        {
            _downloadSemaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<MemoryStream> CreateFilterMarkdownAsync(SearchFilterBaseDto filter, CancellationToken cancellationToken)
    {
        try
        {
            var filterMarkdownStream = await _filterHelper.GetMarkDownAsync(filter, null, cancellationToken);
            return filterMarkdownStream;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create filter markdown");
            throw;
        }
    }
}
