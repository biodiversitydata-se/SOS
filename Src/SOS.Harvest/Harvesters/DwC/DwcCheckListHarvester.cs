﻿using System.Xml.Linq;
using DwC_A;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SOS.Harvest.DarwinCore;
using SOS.Harvest.Harvesters.DwC.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
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
    public class DwcChecklistHarvester : IDwcChecklistHarvester
    {
        private readonly IVerbatimClient _verbatimClient;
        private readonly IFileDownloadService _fileDownloadService;
        private readonly IFileService _fileService;
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly DwcaConfiguration _dwcaConfiguration;
        private readonly ILogger<DwcChecklistHarvester> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="dwcArchiveReader"></param>
        /// <param name="fileDownloadService"></param>
        /// <param name="fileService"></param>
        /// <param name="dataProviderRepository"></param>
        /// <param name="dwcaConfiguration"></param>
        /// <param name="logger"></param>
        public DwcChecklistHarvester(
            IVerbatimClient verbatimClient,
            IFileDownloadService fileDownloadService,
            IFileService fileService,
            IDataProviderRepository dataProviderRepository,
            DwcaConfiguration dwcaConfiguration,
            ILogger<DwcChecklistHarvester> logger)
        {
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
            _fileDownloadService = fileDownloadService ?? throw new ArgumentNullException(nameof(fileDownloadService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
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
        public async Task<HarvestInfo> HarvestChecklistsAsync(
            string archivePath,
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(dataProvider.Identifier, DateTime.Now);
            harvestInfo.Id = dataProvider.ChecklistIdentifier;

            using var dwcArchiveVerbatimRepository = new EventOccurrenceDarwinCoreArchiveVerbatimRepository(
                    dataProvider,
                    _verbatimClient,
                    _logger)
            { TempMode = true };

            try
            {
                _logger.LogDebug($"Start clearing DwC-A checklists for {dataProvider.Identifier}");
                await dwcArchiveVerbatimRepository.DeleteCollectionAsync();
                await dwcArchiveVerbatimRepository.AddCollectionAsync();
                _logger.LogDebug($"Finish clearing DwC-A checklists for {dataProvider.Identifier}");

                _logger.LogDebug($"Start storing DwC-A checklists for {dataProvider.Identifier}");

                using var archiveReader = new ArchiveReader(archivePath, _dwcaConfiguration.ImportPath);
                var dwcArchiveReader = new DwcArchiveReader(dataProvider, 0);
                var checklists = await dwcArchiveReader.ReadSamplingEventArchiveAsync(archiveReader);

                cancellationToken?.ThrowIfCancellationRequested();

                var checklistCount = checklists?.Count() ?? 0;

                if (checklistCount != 0)
                {
                    await dwcArchiveVerbatimRepository.AddManyAsync(checklists);

                    if (dataProvider.UseVerbatimFileInExport)
                    {
                        // Delete non Sampling Event DwC-A files and create a new .zip 
                        _fileService.DeleteFile(Path.Combine(archiveReader.OutputPath, "taxonlist.xml"));
                        _fileService.DeleteFile(Path.Combine(archiveReader.OutputPath, "samplingEventTaxonList.txt"));
                        _fileService.DeleteFile(archivePath);
                        _fileService.CompressDirectory(archiveReader.OutputPath, archivePath);

                        _logger.LogDebug($"Start storing source file for {dataProvider.Identifier}");
                        await using var fileStream = File.OpenRead(archivePath);
                        await dwcArchiveVerbatimRepository.StoreSourceFileAsync(dataProvider.Id, fileStream);
                        _logger.LogDebug($"Finish storing source file for {dataProvider.Identifier}");
                    }

                    _logger.LogDebug($"Finish storing DwC-A checklists for {dataProvider.Identifier}");

                    _logger.LogInformation($"Start permanentize temp collection for {dataProvider.Identifier}");
                    await dwcArchiveVerbatimRepository.PermanentizeCollectionAsync();
                    _logger.LogInformation($"Finish permanentize temp collection for {dataProvider.Identifier}");
                }

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = checklistCount;
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
        public async Task<HarvestInfo> HarvestChecklistsAsync(IJobCancellationToken cancellationToken)
        {
            await Task.Run(() => throw new NotImplementedException("Not implemented for this provider"));
            return null!;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestChecklistsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(provider.ChecklistIdentifier, DateTime.Now)
            {
                Status = RunStatus.Failed
            };
            XDocument emlDocument = null!;

            var datasets = provider.Datasets?.Where(ds => ds.Type.Equals(DataProviderDataset.DatasetType.Checklists));
            if (datasets?.Any() ?? false)
            {
                var datasetCount = datasets.Count();

                if (datasetCount > 1)
                {
                    throw new Exception("Multiple check list dataset not implemented");
                }

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

                    if (!string.IsNullOrEmpty(dataset.EmlUrl)) 
                    {
                        try
                        {
                            // Make up to 3 attempts with 1 sek sleep between the attempts 
                            emlDocument = await PollyHelper.GetRetryPolicy(3, 1000).ExecuteAsync(async () =>
                            {
                                return await _fileDownloadService.GetXmlFileAsync(dataset.EmlUrl);
                            });

                            if (emlDocument != null)
                            {
                                if (DateTime.TryParse(
                                    emlDocument!.Root?.Element("dataset")?.Element("pubDate")?.Value,
                                    out var pubDate))
                                {
                                    // If data set not has changed since last harvest, don't harvest again
                                    if (provider.SourceDate == pubDate.ToUniversalTime())
                                    {
                                        _logger.LogInformation($"Harvest of {provider.Identifier}:{dataset.Identifier} canceled, No new data");
                                        harvestInfo.Status = RunStatus.CanceledSuccess;
                                        return harvestInfo;
                                    }

                                    provider.SourceDate = pubDate;
                                };
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"Error getting EML file for {provider.Identifier}:{dataset.Identifier}");
                        }
                    }
                    
                    var path = Path.Combine(_dwcaConfiguration.ImportPath, $"dwca-{dataset.Identifier}.zip");

                    // Try to get DwcA file from IPT and store it locally
                    if (!await _fileDownloadService.GetFileAndStoreAsync(dataset.DataUrl, path))
                    {
                        return harvestInfo;
                    }

                    // Harvest file
                    harvestInfo = await HarvestChecklistsAsync(path, provider, cancellationToken);

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
                    datsetIndex++;
                }
            }

            return harvestInfo;
        }
    }
}