namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>Identifies observations to exclude from search results.</summary>
public class ExcludeFilterDto
{

    /// <summary>
    /// Exclude observations with listed occurrence id's
    /// </summary>
    public ICollection<string> OccurrenceIds { get; set; }

}


