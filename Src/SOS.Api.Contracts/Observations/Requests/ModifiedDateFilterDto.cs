namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Filter for observation Modified date
/// </summary>
public class ModifiedDateFilterDto
{

    /// <summary>
    /// Changed from
    /// </summary>
    public System.DateTimeOffset? From { get; set; }

    /// <summary>
    /// Changed tp
    /// </summary>
    public System.DateTimeOffset? To { get; set; }

}


