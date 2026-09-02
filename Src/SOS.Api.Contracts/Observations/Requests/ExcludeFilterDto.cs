namespace SOS.Api.Contracts.Observations.Requests;
public class ExcludeFilterDto
{

    /// <summary>
    /// Exclude observations with listed occurrence id's
    /// </summary>
    public ICollection<string> OccurrenceIds { get; set; }

}


