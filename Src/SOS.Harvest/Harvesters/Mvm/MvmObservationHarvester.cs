using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Mvm.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Json;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace SOS.Harvest.Harvesters.Mvm
{
    public class MvmObservationHarvester : ObservationHarvesterBase<MvmSampleVerbatim, int>, IMvmObservationHarvester
    {
        private readonly IMvmObservationService _mvmObservationService;
        private readonly MvmServiceConfiguration _mvmServiceConfiguration;
        private int _idCounter;
        protected int NextId => Interlocked.Increment(ref _idCounter);        

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="mvmObservationService"></param>
        /// <param name="mvmObservationVerbatimRepository"></param>
        /// <param name="mvmServiceConfiguration"></param>
        /// <param name="logger"></param>
        public MvmObservationHarvester(
            IMvmObservationService mvmObservationService,
            IMvmObservationVerbatimRepository mvmObservationVerbatimRepository,
            MvmServiceConfiguration mvmServiceConfiguration,
            ILogger<MvmObservationHarvester> logger) : base("Mvm", mvmObservationVerbatimRepository, logger)
        {            
            _mvmObservationService =
                mvmObservationService ?? throw new ArgumentNullException(nameof(mvmObservationService));
            _mvmServiceConfiguration = mvmServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(mvmServiceConfiguration));
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider dataProvider,
            JobRunModes mode,
            DateTime? fromDate, IJobCancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                throw new NotImplementedException("Not implemented for this provider");
            });
            return null!;
        }        

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            var runStatus = RunStatus.Success;
            var harvestCount = 0;
            (DateTime startDate, long preHarvestCount) initValues = (DateTime.Now, 0);
            try
            {
                bool harvestFileResult = await HarvestMvmJsonFileAsync();
                var filePath = System.IO.Path.Combine(_mvmServiceConfiguration.FolderPath, "MVM/all-full-samples.json");
                if (!File.Exists(filePath))
                {
                    return new HarvestInfo(_provider, initValues.startDate)
                    {
                        End = DateTime.Now,
                        Status = RunStatus.Failed
                    };
                }

                initValues.preHarvestCount = await InitializeHarvestAsync(true);

                // Harvest the example file for debugging.
                //var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //string filePath = Path.Combine(assemblyPath, @"Resources/Data-examples/mvm-biology-samples.json");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                using FileStream fs = File.OpenRead(filePath);
                var jsonStreamReader = new Utf8JsonStreamReader(fs, 512 * 1024);
                var batchSize = 100;
                var currentBatch = new List<MvmSampleVerbatim>(batchSize);
                bool finished = false;

                while (jsonStreamReader.Read() || finished)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    if (jsonStreamReader.CurrentDepth == 1 && jsonStreamReader.TokenType == JsonTokenType.PropertyName)
                    {
                        var propertyName = jsonStreamReader.GetString();
                        if (string.Equals(propertyName, "samples", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!jsonStreamReader.Read())
                                throw new JsonException();
                            if (jsonStreamReader.TokenType == JsonTokenType.StartArray)
                            {
                                while (jsonStreamReader.Read() && jsonStreamReader.TokenType != JsonTokenType.EndArray)
                                {
                                    var sample = jsonStreamReader.Deserialise<MvmSampleVerbatim>(options);
                                    sample.Id = NextId;
                                    currentBatch.Add(sample);
                                    harvestCount += sample.Observations != null ? sample.Observations.Count() : 0;

                                    // When batch is full or we've reached the end, save to MongoDB
                                    if (currentBatch.Count >= batchSize)
                                    {
                                        VerbatimRepository.AddManyAsync(currentBatch).GetAwaiter().GetResult();
                                        currentBatch.Clear();
                                    }

                                    if (_mvmServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                                        harvestCount >= _mvmServiceConfiguration.MaxNumberOfSightingsHarvested)
                                    {
                                        break;
                                    }
                                }

                                // Handle any remaining samples in the last batch
                                if (currentBatch.Any())
                                {
                                    VerbatimRepository.AddMany(currentBatch);                                    
                                    finished = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("MVM harvest was cancelled.");
                runStatus = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to harvest MVM");
                runStatus = RunStatus.Failed;
            }

            return await FinishHarvestAsync(initValues, runStatus, harvestCount, provider.PreviousProcessLimit.GetValueOrDefault(80));
        }            

        public Task<HarvestInfo> HarvestAllObservationsSlowlyAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private string GetMvmHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("MVM Harvest settings:");
            sb.AppendLine($"  Page size: {_mvmServiceConfiguration.MaxReturnedChangesInOnePage}");
            sb.AppendLine(
                $"  Max Number Of Sightings Harvested: {_mvmServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }

        private async Task<bool> HarvestMvmJsonFileAsync()
        {
            var directoryPath = Path.Combine(_mvmServiceConfiguration.FolderPath, "MVM");
            var filePath = Path.Combine(directoryPath, "all-full-samples.json");
            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                var url = $"https://miljodata.slu.se/api/observations-service/v2/all-full-samples/biology?token={_mvmServiceConfiguration.Token}";

                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogError("Failed to fetch MVM file from URL. Status Code: {StatusCode}", response.StatusCode);
                    return false;
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await stream.CopyToAsync(fileStream);

                Logger.LogInformation("MVM file successfully downloaded and saved to {FilePath}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while downloading or saving the MVM file.");
                return false;
            }
        }
    }
}