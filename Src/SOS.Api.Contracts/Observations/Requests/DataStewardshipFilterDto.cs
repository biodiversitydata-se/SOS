namespace SOS.Api.Contracts.Observations.Requests;
public class DataStewardshipFilterDto
{

    /// <summary>
    /// Dataset filter
    /// </summary>
    public ICollection<string> DatasetIdentifiers { get; set; }

}


