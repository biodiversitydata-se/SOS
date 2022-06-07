using System.Xml.Linq;
using DwC_A;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.DarwinCore.Interfaces;
using SOS.Harvest.Harvesters.DwC.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Services.Interfaces;

namespace SOS.Harvest.Harvesters.DwC
{
    /// <summary>
    ///     DwC-A observation harvester.
    /// </summary>
    public class DwcObservationHarvester : IDwcObservationHarvester
    {
        private readonly IVerbatimClient _verbatimClient;
        private readonly IDwcArchiveReader _dwcArchiveReader;
        private readonly IFileDownloadService _fileDownloadService;
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly DwcaConfiguration _dwcaConfiguration;
        private readonly ILogger<DwcObservationHarvester> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="dwcArchiveReader"></param>
        /// <param name="fileDownloadService"></param>
        /// <param name="dataProviderRepository"></param>
        /// <param name="dwcaConfiguration"></param>
        /// <param name="logger"></param>
        public DwcObservationHarvester(
            IVerbatimClient verbatimClient,
            IDwcArchiveReader dwcArchiveReader,
            IFileDownloadService fileDownloadService,
            IDataProviderRepository dataProviderRepository,
            DwcaConfiguration dwcaConfiguration,
            ILogger<DwcObservationHarvester> logger)
        {
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
            _dwcArchiveReader = dwcArchiveReader ?? throw new ArgumentNullException(nameof(dwcArchiveReader));
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

        /// <summary>
        ///     Harvest DwC Archive observations
        /// </summary>
        /// <returns></returns>
        public async Task<HarvestInfo> HarvestObservationsAsync(
            string archivePath,
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(dataProvider.Identifier, DateTime.Now);
            harvestInfo.Id = dataProvider.Identifier;
            using var dwcArchiveVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(
                    dataProvider,
                    _verbatimClient,
                    _logger)
            { TempMode = true };

            try
            {
                _logger.LogDebug($"Start clearing DwC-A observations for {dataProvider.Identifier}");
                await dwcArchiveVerbatimRepository.DeleteCollectionAsync();
                await dwcArchiveVerbatimRepository.AddCollectionAsync();
                _logger.LogDebug($"Finish clearing DwC-A observations for {dataProvider.Identifier}");

                _logger.LogDebug($"Start storing DwC-A observations for {dataProvider.Identifier}");
                var observationCount = 0;
                using var archiveReader = new ArchiveReader(archivePath, _dwcaConfiguration.ImportPath);

                var observationBatches =
                    _dwcArchiveReader.ReadArchiveInBatchesAsync(archiveReader, dataProvider,
                        _dwcaConfiguration.BatchSize);
                await foreach (var verbatimObservationsBatch in observationBatches)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_dwcaConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        observationCount >= _dwcaConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        _logger.LogInformation($"Max observations for {dataProvider.Identifier} reached");
                        break;
                    }

                    observationCount += verbatimObservationsBatch.Count();
                    await dwcArchiveVerbatimRepository.AddManyAsync(verbatimObservationsBatch);
                }

                if (dataProvider.UseVerbatimFileInExport)
                {
                    _logger.LogDebug($"Start storing source file for {dataProvider.Identifier}");
                    await using var fileStream = File.OpenRead(archivePath);
                    await dwcArchiveVerbatimRepository.StoreSourceFileAsync(dataProvider.Id, fileStream);
                    _logger.LogDebug($"Finish storing source file for {dataProvider.Identifier}");
                }

                _logger.LogDebug($"Finish storing DwC-A observations for {dataProvider.Identifier}");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = observationCount;

                _logger.LogInformation($"Start permanentize temp collection for {dataProvider.Identifier}");
                await dwcArchiveVerbatimRepository.PermanentizeCollectionAsync();
                _logger.LogInformation($"Finish permanentize temp collection for {dataProvider.Identifier}");
            }
            catch (JobAbortedException e)
            {
                _logger.LogError(e, $"Canceled harvest of DwC Archive for {dataProvider.Identifier}");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed harvest of DwC Archive for {dataProvider.Identifier}");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for DwcA files");
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for DwcA files");
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
                _logger.LogError(e, $"Failed count number of observations for {dataProvider.Identifier}");
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
            XDocument emlDocument = null;
            _logger.LogInformation($"Start harvesting sightings for {provider.Identifier} data provider. Status={harvestInfo.Status}");

            var downloadUrlEml = provider.DownloadUrls
                ?.FirstOrDefault(p => p.Type.Equals(DownloadUrl.DownloadType.ObservationEml))?.Url;

            if (!string.IsNullOrEmpty(downloadUrlEml))
            {
                try
                {
                    // Try to get eml document from ipt
                    emlDocument = await _fileDownloadService.GetXmlFileAsync(downloadUrlEml);

                    if (emlDocument != null)
                    {
                        if (DateTime.TryParse(
                            emlDocument.Root.Element("dataset").Element("pubDate").Value,
                            out var pubDate))
                        {
                            // If dataset not has changed since last harvest and there exist observations in MongoDB, don't harvest again
                            if (provider.SourceDate == pubDate.ToUniversalTime())
                            {
                                var nrExistingObservations = await GetNumberOfObservationsInExistingCollectionAsync(provider);
                                if (nrExistingObservations.GetValueOrDefault() > 0)
                                {
                                    _logger.LogInformation($"Harvest of {provider.Identifier} canceled, No new data");
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
                    _logger.LogError(e, $"Error getting EML file for {provider.Identifier}");
                }
            }

            var path = Path.Combine(_dwcaConfiguration.ImportPath, $"dwca-{provider.Identifier}.zip");

            // Try to get DwcA file from IPT and store it locally
            var downloadUrl = provider.DownloadUrls
                ?.FirstOrDefault(p => p.Type.Equals(DownloadUrl.DownloadType.Observations))?.Url;
            if (!await _fileDownloadService.GetFileAndStoreAsync(downloadUrl, path))
            {
                return harvestInfo;
            }

            // Harvest file
            harvestInfo = await HarvestObservationsAsync(path, provider, cancellationToken);

            if (harvestInfo.Status == RunStatus.Success && emlDocument != null)
            {
                if (!await _dataProviderRepository.StoreEmlAsync(provider.Id, emlDocument))
                {
                    _logger.LogWarning($"Error updating EML for {provider.Identifier}");
                }
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            _logger.LogInformation($"Finish harvesting sightings for {provider.Identifier} data provider. Status={harvestInfo.Status}");
            return harvestInfo;
        }
    }
}