namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>Identifies the data-stewardship dataset associated with an observation.</summary>
public class DataStewardshipInfo
{

    /// <summary>
    /// Identifier of the dataset.
    /// </summary>
    public string DatasetIdentifier { get; set; }

    /// <summary>
    /// Title of the dataset.
    /// </summary>
    public string DatasetTitle { get; set; }

}


