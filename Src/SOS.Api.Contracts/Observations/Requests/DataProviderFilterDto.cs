namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Data provider filter.
/// </summary>
public class DataProviderFilterDto
{

    /// <summary>
    /// Data provider id's
    /// </summary>
    public ICollection<int> Ids { get; set; }

}


