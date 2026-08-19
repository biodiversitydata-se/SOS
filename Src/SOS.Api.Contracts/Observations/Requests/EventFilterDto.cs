using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Contracts.Observations.Requests;
public class EventFilterDto
{

    /// <summary>
    /// Event id's
    /// </summary>
    public ICollection<string> Ids { get; set; }

}


