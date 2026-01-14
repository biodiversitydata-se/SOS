using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using SOS.Status.Web.Client.Dtos.SosObsApi;
using SOS.Status.Web.Client.JsonConverters;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace SOS.Status.Web.Client.Pages;

public partial class SearchByCursor
{
    private const int MaxUiUpdates = 30;
    
    private int take = 1000;
    private string sortBy = "taxon.id";
    private int maxObservations = 50000;
    private string searchFilterJson = """
{
    "occurrenceStatus": "Present", 
    "geographics" : {     
        "areas" : [
            {
                "areaType": "Municipality",
                "featureId": "687"
            }
        ]
    }
}
""";
    private bool isRunning = false;
    private CancellationTokenSource? cancellationTokenSource;
    private SearchByCursorResult? testResult;

    // Progress tracking
    private int currentPage = 0;
    private int fetchedCount = 0;
    private long totalCount = 0;
    private long effectiveTotalCount = 0;
    private double progressPercent = 0;
    private string statusMessage = "";
    private Color progressColor = Color.Primary;
    private Color statusColor = Color.Default;
    private int estimatedTotalPages = 0;
    private int uiUpdateInterval = 1; // Update UI every N pages

    // Store only minimal data - NOT full Observation objects (saves significant memory)
    private List<ObservationSummary> firstObservations = new(10);
    private List<ObservationSummary> lastObservations = new(10);

    // Static JsonSerializerOptions - thread-safe and reusable
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(),
            new GeoJsonConverter()
        }
    };

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private async Task RunTestAsync()
    {
        isRunning = true;
        testResult = null;
        currentPage = 0;
        fetchedCount = 0;
        totalCount = 0;
        effectiveTotalCount = 0;
        progressPercent = 0;
        estimatedTotalPages = 0;
        uiUpdateInterval = 1;
        statusMessage = "Starting...";
        statusColor = Color.Default;
        progressColor = Color.Primary;
        cancellationTokenSource = new CancellationTokenSource();
        firstObservations.Clear();
        lastObservations.Clear();

        var stopwatch = Stopwatch.StartNew();
        
        // Pre-allocate and REUSE HashSets to avoid repeated allocations
        var previousPageOccurrenceIds = new HashSet<string>(take);
        var currentPageOccurrenceIds = new HashSet<string>(take);
        var duplicates = new List<DuplicateInfo>();
        
        string? cursor = null;
        int pagesFetched = 0;
        string? errorMessage = null;
        bool cancelled = false;
        bool limitReached = false;

        try
        {
            SearchFilterInternalDto? filter;
            try
            {
                filter = JsonSerializer.Deserialize<SearchFilterInternalDto>(searchFilterJson, _jsonSerializerOptions);
                if (filter == null)
                {
                    filter = new SearchFilterInternalDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };
                }
                // Request ONLY needed fields - drastically reduces memory and network
                filter.Output = new OutputFilterExtendedDto
                {
                    Fields = ["occurrence.occurrenceId", "taxon.id", "location.decimalLatitude", "location.decimalLongitude"]
                };
            }
            catch (JsonException ex)
            {
                errorMessage = $"Invalid JSON filter: {ex.Message}";
                testResult = new SearchByCursorResult { ErrorMessage = errorMessage, ElapsedTime = stopwatch.Elapsed };
                return;
            }

            StateHasChanged();

            // Iterate through all pages
            do
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                currentPage++;
                pagesFetched++;
                statusMessage = $"Fetching page {currentPage}{(estimatedTotalPages > 0 ? $"/{estimatedTotalPages}" : "")}...";
                
                // Smart UI update logic - only update when crossing update interval boundaries
                bool shouldUpdateUi = currentPage == 1 || currentPage % uiUpdateInterval == 0;
                
                if (shouldUpdateUi)
                {
                    StateHasChanged();
                    await Task.Yield(); // Yield instead of Delay - less memory overhead
                }

                var resultOrError = await FetchPageAsync(filter, cursor, cancellationTokenSource.Token);

                if (resultOrError.IsFailure)
                {
                    errorMessage = resultOrError.Error;
                    Snackbar.Add($"Error on page {currentPage}: {errorMessage}", Severity.Error);
                    break;
                }

                var result = resultOrError.Value;
                if (result == null)
                {
                    errorMessage = "Received null result from API";
                    break;
                }

                if (currentPage == 1)
                {
                    totalCount = result.TotalCount;
                    effectiveTotalCount = maxObservations > 0 && maxObservations < totalCount 
                        ? maxObservations 
                        : totalCount;
                    
                    // Calculate estimated total pages and UI update interval
                    estimatedTotalPages = (int)Math.Ceiling((double)effectiveTotalCount / take);
                    
                    // Calculate interval to get approximately MaxUiUpdates updates
                    // Always at least 1, but could be higher if many pages
                    uiUpdateInterval = Math.Max(1, estimatedTotalPages / MaxUiUpdates);
                    
                    statusMessage = $"Fetching page {currentPage}/{estimatedTotalPages}... (will update UI every {uiUpdateInterval} pages)";
                    StateHasChanged();
                }

                if (result.Records != null)
                {
                    // Clear and REUSE the HashSet instead of creating new
                    currentPageOccurrenceIds.Clear();
                    
                    int recordIndex = 0;
                    int recordCount = 0;
                    
                    // Process in streaming fashion - enumerate directly without ToList()
                    foreach (var record in result.Records)
                    {
                        recordCount++;
                        var occurrenceId = record.Occurrence?.OccurrenceId;
                        
                        // Store first 10 as lightweight summaries (first page only)
                        if (currentPage == 1 && recordIndex < 10)
                        {
                            firstObservations.Add(ExtractSummary(record));
                        }
                        
                        if (!string.IsNullOrEmpty(occurrenceId))
                        {
                            currentPageOccurrenceIds.Add(occurrenceId);
                            
                            // Check duplicates against previous page only
                            if (previousPageOccurrenceIds.Contains(occurrenceId))
                            {
                                var existing = duplicates.FirstOrDefault(d => d.OccurrenceId == occurrenceId);
                                if (existing != null)
                                {
                                    existing.Count++;
                                    existing.LastSeenOnPage = currentPage;
                                }
                                else
                                {
                                    duplicates.Add(new DuplicateInfo 
                                    { 
                                        OccurrenceId = occurrenceId, 
                                        Count = 2,
                                        FirstSeenOnPage = currentPage - 1,
                                        LastSeenOnPage = currentPage
                                    });
                                }
                            }
                        }
                        recordIndex++;
                    }

                    // Store last 10 - need to re-enumerate but only store summaries
                    lastObservations.Clear();
                    foreach (var record in result.Records.TakeLast(10))
                    {
                        lastObservations.Add(ExtractSummary(record));
                    }

                    // Swap HashSets by reference - avoid creating new allocations
                    (previousPageOccurrenceIds, currentPageOccurrenceIds) = (currentPageOccurrenceIds, previousPageOccurrenceIds);
                    
                    fetchedCount += recordCount;
                    progressPercent = effectiveTotalCount > 0 ? (double)fetchedCount / effectiveTotalCount * 100 : 0;
                }

                // Check if we've reached the limit
                if (maxObservations > 0 && fetchedCount >= maxObservations)
                {
                    limitReached = true;
                    statusMessage = $"Reached limit of {maxObservations} observations";
                    statusColor = Color.Warning;
                    break;
                }                

                cursor = result.NextCursor;

            } while (!string.IsNullOrEmpty(cursor));            

            if (!limitReached && !cancelled)
            {
                statusMessage = "Analyzing results...";
                StateHasChanged();
            }
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
            statusMessage = "Test cancelled by user";
            statusColor = Color.Warning;
        }
        catch (Exception ex)
        {
            errorMessage = GetDetailedErrorMessage(ex);
            statusMessage = $"Error: {errorMessage}";
            statusColor = Color.Error;
            Snackbar.Add($"Error during test: {errorMessage}", Severity.Error);
        }
        finally
        {
            stopwatch.Stop();
            isRunning = false;

            // Help GC by clearing temporary collections
            previousPageOccurrenceIds.Clear();
            currentPageOccurrenceIds.Clear();

            duplicates = duplicates.OrderByDescending(d => d.Count).ToList();

            testResult = new SearchByCursorResult
            {
                TotalFetched = fetchedCount,
                UniqueCount = fetchedCount - duplicates.Sum(d => d.Count - 1),
                DuplicateCount = duplicates.Sum(d => d.Count - 1),
                PagesFetched = pagesFetched,
                ElapsedTime = stopwatch.Elapsed,
                Duplicates = duplicates,
                ErrorMessage = errorMessage,
                Cancelled = cancelled,
                LimitReached = limitReached,
                MaxObservationsLimit = maxObservations,
                Success = string.IsNullOrEmpty(errorMessage) && !cancelled && duplicates.Count == 0
            };

            if (testResult.Success)
            {
                statusMessage = limitReached 
                    ? $"Test completed successfully (limited to {maxObservations} observations) - No duplicates found!"
                    : "Test completed successfully - No duplicates found!";
                statusColor = Color.Success;
                progressColor = Color.Success;
            }
            else if (duplicates.Count > 0)
            {
                statusMessage = $"Test completed - {duplicates.Count} duplicate occurrenceIds found across page boundaries!";
                statusColor = Color.Error;
                progressColor = Color.Error;
            }

            StateHasChanged();
        }
    }

    /// <summary>
    /// Extract minimal data from Observation. Full Observation can be several KB,
    /// ObservationSummary is ~100 bytes.
    /// </summary>
    private static ObservationSummary ExtractSummary(Observation obs) => new()
    {
        OccurrenceId = obs.Occurrence?.OccurrenceId ?? "",
        TaxonId = obs.Taxon?.Id ?? 0,
        DecimalLatitude = obs.Location?.DecimalLatitude,
        DecimalLongitude = obs.Location?.DecimalLongitude
    };

    private async Task<Result<SearchByCursorResultDto<Observation>>> FetchPageAsync(
        SearchFilterInternalDto filter, string? cursor, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await SearchService.SearchObservationsByCursor(filter, take, cursor, sortBy, "Asc");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return Result.Failure<SearchByCursorResultDto<Observation>>(GetDetailedErrorMessage(ex));
        }
    }

    private static string GetDetailedErrorMessage(Exception ex)
    {
        var sb = new StringBuilder();
        var current = ex;
        while (current != null)
        {
            if (sb.Length > 0) sb.Append(" | ");
            sb.Append($"{current.GetType().Name}: {current.Message}");
            if (current is HttpRequestException { StatusCode: not null } httpEx)
                sb.Append($" | Status Code: {(int)httpEx.StatusCode.Value} ({httpEx.StatusCode.Value})");
            current = current.InnerException;
        }
        return sb.ToString();
    }

    private async Task DownloadAsJsonAsync()
    {
        var data = new
        {
            FirstObservations = firstObservations,
            LastObservations = lastObservations,
            Summary = new
            {
                testResult?.TotalFetched, testResult?.UniqueCount, testResult?.DuplicateCount,
                testResult?.PagesFetched, testResult?.ElapsedTime, ExportDateTime = DateTime.UtcNow
            }
        };

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await DownloadFileAsync($"searchbycursor-{DateTime.Now:yyyyMMdd-HHmmss}.json", "application/json", json);
    }

    private async Task DownloadAsCsvAsync()
    {
        var csv = new StringBuilder();
        csv.AppendLine("Section,OccurrenceId,TaxonId,DecimalLatitude,DecimalLongitude");
        foreach (var obs in firstObservations)
            csv.AppendLine($"First,{obs.OccurrenceId},{obs.TaxonId},{obs.DecimalLatitude},{obs.DecimalLongitude}");
        foreach (var obs in lastObservations)
            csv.AppendLine($"Last,{obs.OccurrenceId},{obs.TaxonId},{obs.DecimalLatitude},{obs.DecimalLongitude}");

        await DownloadFileAsync($"searchbycursor-{DateTime.Now:yyyyMMdd-HHmmss}.csv", "text/csv", csv.ToString());
    }

    private async Task DownloadFileAsync(string fileName, string contentType, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var base64 = Convert.ToBase64String(bytes);
        await JSRuntime.InvokeVoidAsync("downloadFile", fileName, contentType, base64);
    }

    private void CancelTest() => cancellationTokenSource?.Cancel();

    private void ClearResults()
    {
        testResult = null;
        currentPage = 0;
        fetchedCount = 0;
        totalCount = 0;
        effectiveTotalCount = 0;
        progressPercent = 0;
        statusMessage = "";
        estimatedTotalPages = 0;
        uiUpdateInterval = 1;
        firstObservations.Clear();
        lastObservations.Clear();
    }

    /// <summary>
    /// Lightweight observation summary - ~100 bytes vs several KB for full Observation
    /// </summary>
    public class ObservationSummary
    {
        public string OccurrenceId { get; set; } = "";
        public int TaxonId { get; set; }
        public double? DecimalLatitude { get; set; }
        public double? DecimalLongitude { get; set; }
    }

    public class SearchByCursorResult
    {
        public int TotalFetched { get; set; }
        public int UniqueCount { get; set; }
        public int DuplicateCount { get; set; }
        public int PagesFetched { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public List<DuplicateInfo> Duplicates { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool Cancelled { get; set; }
        public bool LimitReached { get; set; }
        public int MaxObservationsLimit { get; set; }
        public bool Success { get; set; }
    }

    public class DuplicateInfo
    {
        public string OccurrenceId { get; set; } = "";
        public int Count { get; set; }
        public int FirstSeenOnPage { get; set; }
        public int LastSeenOnPage { get; set; }
    }
}
