using DwC_A;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.DarwinCore;
using SOS.Harvest.Harvesters.DwC.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Services.Interfaces;
using System.Xml.Linq;

namespace SOS.Harvest.Harvesters.DwC
{
    /// <summary>
    ///     DwC-A observation harvester.
    /// </summary>
    public class DwcObservationHarvester : IDwcObservationHarvester
    {
        private readonly IVerbatimClient _verbatimClient;
        private readonly IFileDownloadService _fileDownloadService;
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly DwcaConfiguration _dwcaConfiguration;
        private readonly ILogger<DwcObservationHarvester> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="fileDownloadService"></param>
        /// <param name="dataProviderRepository"></param>
        /// <param name="dwcaConfiguration"></param>
        /// <param name="logger"></param>
        public DwcObservationHarvester(
            IVerbatimClient verbatimClient,
            IFileDownloadService fileDownloadService,
            IDataProviderRepository dataProviderRepository,
            DwcaConfiguration dwcaConfiguration,
            ILogger<DwcObservationHarvester> logger)
        {
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
            _fileDownloadService = fileDownloadService ?? throw new ArgumentNullException(nameof(fileDownloadService));
            _dataProviderRepository =
                dataProviderRepository ?? throw new ArgumentNullException(nameof(dataProviderRepository));
            _dwcaConfiguration = dwcaConfiguration ?? throw new ArgumentNullException(nameof(dwcaConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get EML XML document.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <returns></returns>
        public XDocument GetEmlXmlDocument(string archivePath)
        {
            using var archiveReader = new ArchiveReader(archivePath, _dwcaConfiguration.ImportPath);
            var emlFile = archiveReader.GetEmlXmlDocument();
            return emlFile;
        }

        public async Task<HarvestInfo> HarvestObservationsAsync(
           string archivePath,
           DataProvider dataProvider,
           bool initializeTempCollection,
           bool permanentizeTempCollection,
           IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(dataProvider.Identifier, DateTime.Now);
            harvestInfo.Id = dataProvider.Identifier;
            using var dwcArchiveVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(dataProvider, _verbatimClient, _logger) { TempMode = true };

            try
            {
                if (initializeTempCollection)
                {
                    _logger.LogDebug($"Start clearing DwC-A observations for {dataProvider.Identifier}");
                    await dwcArchiveVerbatimRepository.DeleteCollectionAsync();
                    await dwcArchiveVerbatimRepository.AddCollectionAsync();
                    _logger.LogDebug($"Finish clearing DwC-A observations for {dataProvider.Identifier}");

                    _logger.LogDebug($"Start storing DwC-A observations for {dataProvider.Identifier}");
                }
                var dwcArchiveReader = new DwcArchiveReader(dataProvider, await dwcArchiveVerbatimRepository.GetMaxIdAsync());
                var observationCount = 0;
                using var archiveReader = new ArchiveReader(archivePath, _dwcaConfiguration.ImportPath);

                var observationBatches =
                    dwcArchiveReader.ReadArchiveInBatchesAsync(archiveReader,
                        _dwcaConfiguration.BatchSize);
                await foreach (var verbatimObservationsBatch in observationBatches)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    if (!verbatimObservationsBatch?.Any() ?? true)
                    {
                        continue;
                    }

                    if (_dwcaConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        observationCount >= _dwcaConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        _logger.LogInformation("Max observations for {@dataProvider} reached", dataProvider.Identifier);
                        break;
                    }

                    observationCount += verbatimObservationsBatch!.Count();
                    await dwcArchiveVerbatimRepository.AddManyAsync(verbatimObservationsBatch);
                }

                if (dataProvider.UseVerbatimFileInExport && (dataProvider.Datasets?.Any() ?? false)) // If no dataset, file must have been manually uploaded and is allready stored
                {
                    _logger.LogDebug($"Start storing source file for {dataProvider.Identifier}");
                    await using var fileStream = File.OpenRead(archivePath);
                    await dwcArchiveVerbatimRepository.StoreSourceFileAsync(dataProvider.Id, fileStream);
                    _logger.LogDebug($"Finish storing source file for {dataProvider.Identifier}");
                }

                if (permanentizeTempCollection)
                {
                    _logger.LogDebug($"Finish storing DwC-A observations for {dataProvider.Identifier}");
                    _logger.LogInformation("Start permanentize temp collection for {@dataProvider}", dataProvider.Identifier);
                    await dwcArchiveVerbatimRepository.PermanentizeCollectionAsync();
                    _logger.LogInformation("Finish permanentize temp collection for {@dataProvider}", dataProvider.Identifier);
                }

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = observationCount;
            }
            catch (JobAbortedException e)
            {
                _logger.LogError(e, "Canceled harvest of DwC Archive for {@dataProvider}", dataProvider.Identifier);
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of DwC Archive for {@dataProvider}", dataProvider.Identifier);
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo; 
        }

        /// <summary>
        ///     Harvest DwC Archive observations
        /// </summary>
        /// <returns></returns>
        public async Task<HarvestInfo> HarvestObservationsAsync(
            string archivePath,
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            return await HarvestObservationsAsync(archivePath, dataProvider, true, true, cancellationToken);
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                throw new NotImplementedException("Not implemented for DwcA files");
            });
            return null!;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider dataProvider,
            JobRunModes mode,
            DateTime? fromDate, IJobCancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                throw new NotImplementedException("Not implemented for DwcA files");
            });
            return null!;
        }

        private async Task<int?> GetNumberOfObservationsInExistingCollectionAsync(DataProvider dataProvider)
        {
            using var dwcArchiveVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(
                    dataProvider,
                    _verbatimClient,
                    _logger)
            { TempMode = false };

            try
            {
                bool collectionExists = await dwcArchiveVerbatimRepository.CheckIfCollectionExistsAsync();
                if (!collectionExists) return null;
                long count = await dwcArchiveVerbatimRepository.CountAllDocumentsAsync();
                return Convert.ToInt32(count);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed count number of observations for {@dataProvider}", dataProvider.Identifier);
                return null;
            }
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(provider.Identifier, DateTime.Now)
            {
                Status = RunStatus.Failed
            };
            XDocument emlDocument = null!;
            _logger.LogInformation("Start harvesting sightings for {@dataProvider} data provider.", provider.Identifier);

            var datasets = provider.Datasets?.Where(ds => ds.Type.Equals(DataProviderDataset.DatasetType.Observations));
            if (datasets?.Any() ?? false)
            {
                var datasetCount = datasets.Count();
                var datsetIndex = 1;
                foreach (var dataset in datasets)
                {
                    if (string.IsNullOrEmpty(dataset.DataUrl))
                    {
                        // Since download url is missing. Assume this dataset is manually handled
                        harvestInfo.Count = -1;
                        harvestInfo.Status = RunStatus.Success;
                        continue;
                    }

                    if (!string.IsNullOrEmpty(dataset.EmlUrl) && datasetCount.Equals(1)) // Todo handle multiple EML
                    {
                        try
                        {
                            // Try to get eml document from ipt
                            emlDocument = await _fileDownloadService.GetXmlFileAsync(dataset.EmlUrl);
                            if (emlDocument != null) // Todo handle multiple EML
                            {
                                if (DateTime.TryParse(
                                    emlDocument?.Root?.Element("dataset")?.Element("pubDate")?.Value,
                                    out var pubDate))
                                {
                                    // If dataset not has changed since last harvest and there exist observations in MongoDB, don't harvest again
                                    if (provider.SourceDate == pubDate.ToUniversalTime() && !_dwcaConfiguration.ForceHarvestUnchangedDwca)
                                    {
                                        var nrExistingObservations = await GetNumberOfObservationsInExistingCollectionAsync(provider);
                                        if (nrExistingObservations.GetValueOrDefault() > 0)
                                        {
                                            _logger.LogInformation("Harvest of {@dataProvider}:{@dataset} canceled, No new data", provider.Identifier, dataset.Identifier);
                                            harvestInfo.Status = RunStatus.CanceledSuccess;
                                            return harvestInfo;
                                        }
                                    }

                                    provider.SourceDate = pubDate;
                                };
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Error getting EML file for {@dataProvider}:{@dataset}", provider.Identifier, dataset.Identifier);
                        }
                    }

                    var path = Path.Combine(_dwcaConfiguration.ImportPath, $"dwca-{dataset.Identifier}.zip");

                    if (!await _fileDownloadService.GetFileAndStoreAsync(dataset.DataUrl, path, "application/zip"))
                    {
                        return harvestInfo;
                    }

                    // Harvest file
                    var latestHarvestInfo = await HarvestObservationsAsync(path, provider, datsetIndex == 1, datsetIndex == datasetCount, cancellationToken);

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    if (!new[] { RunStatus.Success, RunStatus.CanceledSuccess }.Contains(latestHarvestInfo.Status))
                    {
                        return latestHarvestInfo;
                    }
                    harvestInfo.Status = latestHarvestInfo.Status;
                    harvestInfo.Count += latestHarvestInfo.Count;
                    if (latestHarvestInfo.DataLastModified > harvestInfo.DataLastModified)
                    {
                        harvestInfo.DataLastModified = latestHarvestInfo.DataLastModified;
                    }
                    harvestInfo.End = latestHarvestInfo.End;

                    datsetIndex++;
                }
            }

            if (harvestInfo.Status == RunStatus.Success && emlDocument != null)
            {
                if (!await _dataProviderRepository.StoreEmlAsync(provider.Id, emlDocument))
                {
                    _logger.LogWarning("Error updating EML for {@dataProvider}", provider.Identifier);
                }
            }

            _logger.LogInformation("Finish harvesting sightings for {@dataProvider} data provider. Status={@harvestStatus}", provider.Identifier, harvestInfo?.Status);
            return harvestInfo!;
        }

        public Task<HarvestInfo> HarvestAllObservationsSlowlyAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}