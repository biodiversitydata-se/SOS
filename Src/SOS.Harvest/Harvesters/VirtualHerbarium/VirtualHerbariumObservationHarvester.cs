using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.VirtualHerbarium.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Lib.Services.Interfaces;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;

namespace SOS.Harvest.Harvesters.VirtualHerbarium;

/// <summary>
///     Harvester for Virtual Herbarium observations using NDJSON format
/// </summary>
public class VirtualHerbariumObservationHarvester : 
    ObservationHarvesterBase<VirtualHerbariumObservationVerbatim, int>, 
    IVirtualHerbariumObservationHarvester
{
    private const string ZipFileUrl = "https://api.virtuellaherbariet.se/vhapi/admin/sweden-occurrences.zip";
    private const string NdjsonFileName = "sweden-occurrences.ndjson";
    private const string TestNdjsonFileName = "sweden-occurrences-test.ndjson";
    
    private readonly IFileDownloadService _fileDownloadService;
    private readonly VirtualHerbariumServiceConfiguration _virtualHerbariumServiceConfiguration;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="fileDownloadService"></param>
    /// <param name="virtualHerbariumObservationVerbatimRepository"></param>
    /// <param name="virtualHerbariumServiceConfiguration"></param>
    /// <param name="logger"></param>
    public VirtualHerbariumObservationHarvester(
        IFileDownloadService fileDownloadService,
        IVirtualHerbariumObservationVerbatimRepository virtualHerbariumObservationVerbatimRepository,
        VirtualHerbariumServiceConfiguration virtualHerbariumServiceConfiguration,
        ILogger<VirtualHerbariumObservationHarvester> logger) : base("Virtual Herbarium", virtualHerbariumObservationVerbatimRepository, logger)
    {
        _fileDownloadService = fileDownloadService ??
                               throw new ArgumentNullException(nameof(fileDownloadService));
        _virtualHerbariumServiceConfiguration = virtualHerbariumServiceConfiguration ??
                                                throw new ArgumentNullException(nameof(virtualHerbariumServiceConfiguration));
        
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
    }

    /// <inheritdoc />
    public Task<HarvestInfo> HarvestAllObservationsSlowlyAsync(DataProvider provider, IJobCancellationToken cancellationToken)
    {
        throw new NotImplementedException("Not implemented for this provider");
    }

    /// <inheritdoc />
    public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider dataProvider, JobRunModes mode, DateTime? fromDate, IJobCancellationToken cancellationToken)
    {
        await Task.Run(() => throw new NotImplementedException("Not implemented for this provider"));
        return null!;
    }

    /// <inheritdoc />
    public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
    {
        // If using test file, read from Resources/Data-examples/
        if (_virtualHerbariumServiceConfiguration.UseTestFile)
        {
            Logger.LogInformation("Using test file from Resources/Data-examples/: {FileName}", TestNdjsonFileName);
            
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var testFilePath = Path.Combine(assemblyPath, "Resources", "Data-examples", TestNdjsonFileName);

            if (!File.Exists(testFilePath))
            {
                throw new FileNotFoundException(
                    $"Test file not found: {testFilePath}. " +
                    $"Make sure {TestNdjsonFileName} exists in Resources/Data-examples/ and is copied to output directory.");
            }

            Logger.LogInformation("Reading test file from: {FilePath}", testFilePath);
            return await HarvestFromStreamAsync(
                () => FileSystemHelper.WaitForFileAndThenOpenIt(testFilePath),
                provider,
                cancellationToken);
        }

        // Production mode: download from API
        string? tempZipPath = null;
        string? tempExtractPath = null;
        
        try
        {
            // Create temp paths
            tempZipPath = Path.Combine(Path.GetTempPath(), $"vh-{Guid.NewGuid()}.zip");
            tempExtractPath = Path.Combine(Path.GetTempPath(), $"vh-{Guid.NewGuid()}");
            
            // Download the zip file
            Logger.LogInformation("Downloading Virtual Herbarium zip file from: {Url}", ZipFileUrl);
            var downloadSuccess = await _fileDownloadService.GetFileAndStoreAsync(ZipFileUrl, tempZipPath, "application/zip");
            
            if (!downloadSuccess)
            {
                throw new Exception($"Failed to download zip file from: {ZipFileUrl}");
            }
            
            Logger.LogInformation("Successfully downloaded zip file to: {Path}", tempZipPath);
            
            // Extract the zip file
            Directory.CreateDirectory(tempExtractPath);
            ZipFile.ExtractToDirectory(tempZipPath, tempExtractPath);
            
            // Find the ndjson file
            var ndjsonFilePath = Path.Combine(tempExtractPath, NdjsonFileName);
            
            if (!File.Exists(ndjsonFilePath))
            {
                throw new FileNotFoundException($"NDJSON file not found in zip archive: {NdjsonFileName}");
            }
            
            Logger.LogInformation("Starting Virtual Herbarium harvest from downloaded file: {FilePath}", ndjsonFilePath);
            
            return await HarvestFromStreamAsync(
                () => File.OpenRead(ndjsonFilePath),
                provider,
                cancellationToken);
        }
        finally
        {
            // Cleanup temp files
            try
            {
                if (tempZipPath != null && File.Exists(tempZipPath))
                {
                    File.Delete(tempZipPath);
                }
                if (tempExtractPath != null && Directory.Exists(tempExtractPath))
                {
                    Directory.Delete(tempExtractPath, true);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to cleanup temp files");
            }
        }
    }

    /// <inheritdoc />
    public async Task<HarvestInfo> HarvestObservationsFromFileAsync(string filePath, DataProvider dataProvider, IJobCancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"NDJSON file not found: {filePath}");
        }

        Logger.LogInformation("Starting Virtual Herbarium harvest from file: {FilePath}", filePath);             

        return await HarvestFromStreamAsync(
            () => File.OpenRead(filePath),
            dataProvider,
            cancellationToken);
    }

    /// <summary>
    ///     Harvest observations from a stream (can be file or embedded resource)
    /// </summary>
    private async Task<HarvestInfo> HarvestFromStreamAsync(
        Func<Stream> streamFactory,
        DataProvider provider,
        IJobCancellationToken cancellationToken)
    {
        var occurrenceIdsSet = new HashSet<string>();
        var runStatus = RunStatus.Success;
        var harvestCount = 0;
        (DateTime startDate, long preHarvestCount) initValues = (DateTime.Now, 0);

        try
        {
            initValues.preHarvestCount = await InitializeHarvestAsync(true);

            using var stream = streamFactory();
            using var reader = new StreamReader(stream);

            var batch = new List<VirtualHerbariumObservationVerbatim>();
            const int batchSize = 1000;
            var lineNumber = 0;
            var id = 1;

            string? line;
            while ((line = await reader.ReadLineAsync()) is not null)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                try
                {
                    var verbatim = JsonSerializer.Deserialize<VirtualHerbariumObservationVerbatim>(line, _jsonSerializerOptions);
                    
                    if (verbatim == null)
                    {
                        Logger.LogWarning("Failed to deserialize line {LineNumber} - result was null", lineNumber);
                        continue;
                    }

                    // Generate unique occurrence ID
                    var occurrenceId = $"{verbatim.InstCode}#{verbatim.AccessionNo}#{verbatim.TaxaName}";
                    
                    if (occurrenceIdsSet.Contains(occurrenceId))
                    {
                        Logger.LogWarning("Duplicate observation found in VirtualHerbarium: {OccurrenceId}", occurrenceId);
                        continue;
                    }

                    occurrenceIdsSet.Add(occurrenceId);
                    verbatim.Id = id++;
                    batch.Add(verbatim);
                    harvestCount++;

                    // Store batch when it reaches batch size
                    if (batch.Count >= batchSize)
                    {
                        Logger.LogDebug("Storing batch of {Count} Virtual Herbarium observations", batch.Count);
                        await VerbatimRepository.AddManyAsync(batch);
                        batch.Clear();
                    }

                    // Check max harvest limit
                    if (_virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        harvestCount >= _virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        Logger.LogInformation("Reached max harvest limit of {MaxLimit}", _virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested);
                        break;
                    }
                }
                catch (JsonException ex)
                {
                    Logger.LogWarning(ex, "Failed to parse JSON on line {LineNumber}", lineNumber);
                }
            }

            // Store remaining items in batch
            if (batch.Count > 0)
            {
                Logger.LogDebug("Storing final batch of {Count} Virtual Herbarium observations", batch.Count);
                await VerbatimRepository.AddManyAsync(batch);
            }

            Logger.LogInformation("Harvested {Count} Virtual Herbarium observations", harvestCount);
        }
        catch (JobAbortedException)
        {
            Logger.LogInformation("Virtual Herbarium harvest was cancelled");
            runStatus = RunStatus.Canceled;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to harvest Virtual Herbarium");
            runStatus = RunStatus.Failed;
        }

        return await FinishHarvestAsync(initValues, runStatus, harvestCount, provider.PreviousProcessLimit.GetValueOrDefault(80));
    }

    /// <summary>
    ///     Reads observations from an NDJSON file
    /// </summary>
    /// <param name="filePath">Path to the NDJSON file</param>
    /// <returns>Enumerable of verbatim observations</returns>
    public async IAsyncEnumerable<VirtualHerbariumObservationVerbatim> ReadObservationsFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"NDJSON file not found: {filePath}");
        }

        using var reader = new StreamReader(filePath);
        var lineNumber = 0;
        var id = 1;

        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            VirtualHerbariumObservationVerbatim? verbatim = null;
            try
            {
                verbatim = JsonSerializer.Deserialize<VirtualHerbariumObservationVerbatim>(line, _jsonSerializerOptions);
            }
            catch (JsonException ex)
            {
                Logger.LogWarning(ex, "Failed to parse JSON on line {LineNumber}", lineNumber);
            }

            if (verbatim != null)
            {
                verbatim.Id = id++;
                yield return verbatim;
            }
        }
    }
}