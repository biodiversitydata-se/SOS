using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>Filters observations by survey event identifiers.</summary>
public class EventFilterDto
{

    /// <summary>
    /// Event id's
    /// </summary>
    public ICollection<string> Ids { get; set; }

}


