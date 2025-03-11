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
        /// Save absent observation
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="dwcCollectionRepository"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        private async Task<int> CreateAndStoreAbsentObservations(DataProvider dataProvider, DwcCollectionRepository dwcCollectionRepository, List<DwcEventOccurrenceVerbatim>? events)
        {
            try
            {
                if (!events?.Any() ?? true)
                {
                    return 0;
                }

                _logger.LogDebug("Start storing absent DwC-A occurrences for {@dataProvider}", dataProvider.Identifier);
                // dwcCollectionRepository.OccurrenceRepository.TempMode = false;
                int observationCount = 0;
                var batchAbsentObservations = new List<DwcObservationVerbatim>();
                var id = await dwcCollectionRepository.OccurrenceRepository.GetMaxIdAsync();
                _logger.LogDebug("MaxId={@maxSightingId} before adding absent observations", id);
                for (int i = 0; i < events!.Count; i++)
                {
                    DwcEventOccurrenceVerbatim? ev = events[i];
                    ev.Observations = null; // todo - handle this logic in the DwC-A parser.
                    var absentObservations = ev.CreateAbsentObservations();
                    foreach (var observation in absentObservations)
                    {
                        observation.Id = ++id;
                    }
                    batchAbsentObservations.AddRange(absentObservations);

                    // store batch of absent observations if this is the last iterated event or batch is larger than 10 000.
                    if (i == events.Count - 1 || batchAbsentObservations.Count > 10000)
                    {
                        observationCount += batchAbsentObservations.Count();
                        await dwcCollectionRepository.OccurrenceRepository.AddManyAsync(batchAbsentObservations);
                        batchAbsentObservations.Clear();
                    }
                }

                _logger.LogDebug("Finish storing absent DwC-A occurrences for {@dataProvider}", dataProvider.Identifier);
                return observationCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing absent observations for {@dataProvider}", dataProvider.Identifier);
            }
            finally
            {
                dwcCollectionRepository.OccurrenceRepository.TempMode = true;
            }

            return 0;
        }

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
                using var archiveReader = new ArchiveReader(archivePath, _dwcaConfiguration.ImportPath);
                var dwcCollectionArchiveReaderContext = ArchiveReaderContext.Create(archiveReader, dataProvider, _dwcaConfiguration);
                var dwcCollectionRepository = new DwcCollectionRepository(dataProvider, _verbatimClient, _logger);
                dwcCollectionRepository.BeginTempMode();

                if (initializeTempCollection)
                {
                    _logger.LogDebug("Clear DwC-A observations for {@dataProvider}", dataProvider.Identifier);
                    await dwcCollectionRepository.DeleteCollectionsAsync();
                    await dwcCollectionRepository.OccurrenceRepository.AddCollectionAsync();
                    _logger.LogDebug("Start storing DwC-A observations for {@dataProvider}", dataProvider.Identifier);
                }

                // Read datasets
                List<Lib.Models.Processed.DataStewardship.Dataset.DwcVerbatimDataset>? datasets = null;
                var dwcArchiveReader = new DwcArchiveReader(dataProvider, await dwcCollectionRepository.DatasetRepository.GetMaxIdAsync());
                try
                {
                    datasets = await dwcArchiveReader.ReadDatasetsAsync(dwcCollectionArchiveReaderContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading DwC-A datasets for {@dataProvider}", dataProvider.Identifier);
                }

                // Read observations
                int observationCount = 0;
                dwcArchiveReader = new DwcArchiveReader(dataProvider, await dwcCollectionRepository.OccurrenceRepository.GetMaxIdAsync());
                var observationBatches = dwcArchiveReader.ReadOccurrencesInBatchesAsync(dwcCollectionArchiveReaderContext);
                await foreach (var verbatimObservationsBatch in observationBatches)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_dwcaConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        observationCount >= _dwcaConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        _logger.LogInformation("Max observations for {@dataProvider} reached", dataProvider.Identifier);
                        break;
                    }

                    observationCount += verbatimObservationsBatch!.Count();
                    await dwcCollectionRepository.OccurrenceRepository.AddManyAsync(verbatimObservationsBatch);
                }

                // Read events
                try
                {
                    _logger.LogInformation("Start reading DwC-A events for {@dataProvider}", dataProvider.Identifier);
                    dwcArchiveReader = new DwcArchiveReader(dataProvider, await dwcCollectionRepository.EventRepository.GetMaxIdAsync());
                    var events = (await dwcArchiveReader.ReadEventsAsync(dwcCollectionArchiveReaderContext))?.ToList();
                    if (events != null && events.Any())
                    {
                        _logger.LogInformation("Read {@eventCount} events for {@dataProvider}", events.Count, dataProvider.Identifier);
                        observationCount += await CreateAndStoreAbsentObservations(dataProvider, dwcCollectionRepository, events);
                        if (initializeTempCollection)
                        {
                            _logger.LogDebug("Start storing DwC-A events for {@dataProvider}", dataProvider.Identifier);
                            await dwcCollectionRepository.EventRepository.AddCollectionAsync();
                        }
                        await dwcCollectionRepository.EventRepository.AddManyAsync(events);
                    }
                    else
                    {
                        _logger.LogInformation("Read 0 events for {@dataProvider}", dataProvider.Identifier);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading DwC-A events for {@dataProvider}", dataProvider.Identifier);
                }

                // Save datasets lasts, since DataSet.EventIds could been changed after reading Events
                try
                {
                    if (datasets != null && datasets.Any())
                    {
                        if (initializeTempCollection)
                        {
                            _logger.LogDebug("Start storing DwC-A datasets for {@dataProvider}", dataProvider.Identifier);
                            await dwcCollectionRepository.DatasetRepository.AddCollectionAsync();
                        }
                        await dwcCollectionRepository.DatasetRepository.AddManyAsync(datasets);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading DwC-A datasets for {@dataProvider}", dataProvider.Identifier);
                }

                if (permanentizeTempCollection)
                {
                    if (await dwcCollectionRepository.OccurrenceRepository.PermanentizeCollectionAsync())
                    {
                        _logger.LogDebug("Finish storing DwC-A observations for {@dataProvider}", dataProvider.Identifier);
                    }
                    if (await dwcCollectionRepository.EventRepository.PermanentizeCollectionAsync())
                    {
                        _logger.LogDebug("Finish storing DwC-A events for {@dataProvider}", dataProvider.Identifier);
                    }
                    if (await dwcCollectionRepository.DatasetRepository.PermanentizeCollectionAsync())
                    {
                        _logger.LogDebug("Finish storing DwC-A datasets for {@dataProvider}", dataProvider.Identifier);
                    }
                }

                dwcCollectionRepository.EndTempMode();

                if (dataProvider.UseVerbatimFileInExport && (dataProvider.Datasets?.Any() ?? false)) // If no dataset, file must have been manually uploaded and is allready stored
                {
                    _logger.LogDebug("Start storing source file for {@dataProvider}", dataProvider.Identifier);
                    await using var fileStream = File.OpenRead(archivePath);
                    await dwcArchiveVerbatimRepository.StoreSourceFileAsync(dataProvider.Id, fileStream);
                    _logger.LogDebug("Finish storing source file for {@dataProvider}", dataProvider.Identifier);
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