using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using System.Text.RegularExpressions;

namespace SOS.ContainerIntegrationTests.Helpers;
internal static class DebugTestResultHelper
{
    public static async Task<List<TestResultItem>> CreateTestResultSummaryAsync(
            HttpClient httpClient,
            IList<ArtportalenObservationVerbatim> verbatimObservations,
            IEnumerable<Observation> resultObservations)
    {        
        List<Observation> allProcessedObservations = await GetAllProcessedObservationsAsync(httpClient);
        return CreateTestResultSummary(verbatimObservations, allProcessedObservations, resultObservations);
    }

    public static List<TestResultItem> CreateTestResultSummary(                        
            IList<ArtportalenObservationVerbatim> verbatimObservations,
            IEnumerable<Observation> processedObservations,
            IEnumerable<Observation> resultObservations)
    {
        List<TestResultItem> testResultItems = new List<TestResultItem>();        
        foreach (var item in processedObservations)
        {
            int sightingId = GetSightingIdFromOccurrenceId(item.Occurrence.OccurrenceId);
            var verbatim = verbatimObservations.Single(m => m.SightingId == sightingId);
            bool hit = resultObservations.Any(m => m.Occurrence.OccurrenceId == item.Occurrence.OccurrenceId);
            testResultItems.Add(new TestResultItem
            {
                Hit = hit,
                ProcessedObservation = item,
                VerbatimObservation = verbatim
            });
        }

        return testResultItems;
    }

    private static int GetSightingIdFromOccurrenceId(string occurrenceId)
    {
        string lastInteger = Regex.Match(occurrenceId, @"\d+", RegexOptions.RightToLeft).Value;
        return int.Parse(lastInteger);
    }

    private static async Task<List<Observation>> GetAllProcessedObservationsAsync(HttpClient httpClient)
    {
        var searchFilter = new SearchFilterDto {
            Output = new OutputFilterDto {
                FieldSet = Lib.Enums.OutputFieldSet.All
            }
        };

        var response = await httpClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        return result!.Records.ToList();
    }    
}

public class TestResultItem
{
    public ArtportalenObservationVerbatim? VerbatimObservation { get; set; }
    public Observation? ProcessedObservation { get; set; }
    public object? VerbatimValue { get; set; }
    public object? ProcessedValue { get; set; }
    public bool Hit { get; set; }

    public override string ToString()
    {
        return $"Value={ProcessedValue}, Hit={Hit}";
    }
}
