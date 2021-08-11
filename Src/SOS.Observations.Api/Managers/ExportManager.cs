using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    /// Manager for exports
    /// </summary>
    public class ExportManager : IExportManager
    {
        private readonly IFilterManager _filterManager;
        private readonly IDwcArchiveFileWriter _dwcArchiveFileWriter;
        private readonly IExcelFileWriter _excelWriter;
        private readonly IGeoJsonFileWriter _geoJsonWriter;
        private readonly string _exportPath;
        private readonly ILogger<ExportManager> _logger;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IProcessInfoRepository _processInfoRepository;

        /// <summary>
        ///     Create a Darwin Core Archive file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Path to created file</returns>
        private async Task<string> CreateDWCExportAsync(SearchFilter filter, string fileName,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var processInfo = await _processInfoRepository.GetAsync(_processedObservationRepository.PublicIndexName);
                var fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions();
                var zipFilePath = await _dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                    DataProvider.FilterSubsetDataProvider,
                    filter,
                    fileName,
                    _processedObservationRepository,
                    fieldDescriptions,
                    processInfo,
                    _exportPath,
                    cancellationToken);
                cancellationToken?.ThrowIfCancellationRequested();

                return zipFilePath;
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

        private async Task<string> CreateExcelExportAsync(SearchFilter filter, string fileName,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var zipFilePath = await _excelWriter.CreateFileAync(
                    filter,
                    _exportPath,
                    fileName,
                    cancellationToken);

                return zipFilePath;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Export sightings to Excel was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export sightings to Excel");
                throw;
            }
        }

        private async Task<string> CreateGeoJsonExportAsync(SearchFilter filter, string fileName,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var zipFilePath = await _geoJsonWriter.CreateFileAync(
                   filter,
                   _exportPath,
                   fileName,
                    cancellationToken);

                return zipFilePath;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Export sightings to GeoJson was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export sightings to GeoJson");
                throw;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dwcArchiveFileWriter"></param>
        /// <param name="excelWriter"></param>
        /// <param name="geoJsonWriter"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="filterManager"></param>
        /// <param name="logger"></param>
        public ExportManager(
            IDwcArchiveFileWriter dwcArchiveFileWriter,
            IExcelFileWriter excelWriter,
            IGeoJsonFileWriter geoJsonWriter,
            IProcessedObservationRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            IFilterManager filterManager,
            ILogger<ExportManager> logger)
        {
            _dwcArchiveFileWriter =
                dwcArchiveFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriter));
            _excelWriter =
                excelWriter ?? throw new ArgumentNullException(nameof(excelWriter));
            _geoJsonWriter =
                geoJsonWriter ?? throw new ArgumentNullException(nameof(geoJsonWriter));

            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _processInfoRepository =
                processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<string> CreateExportFileAsync(SearchFilter filter,
            ExportFormat exportFormat,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                await _filterManager.PrepareFilter(null, filter);

                var zipFilePath = exportFormat switch
                {
                    ExportFormat.DwC => await CreateDWCExportAsync(filter, Guid.NewGuid().ToString(), cancellationToken),
                    ExportFormat.Excel => await CreateExcelExportAsync(filter, Guid.NewGuid().ToString(), cancellationToken),
                    ExportFormat.GeoJson => await CreateGeoJsonExportAsync(filter, Guid.NewGuid().ToString(), cancellationToken)
                };

                // zend file to user
                return zipFilePath;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create export file");
                return null;
            }
        }
    }
}
