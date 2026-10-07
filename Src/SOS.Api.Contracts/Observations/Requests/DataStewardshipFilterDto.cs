namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>Filters observations by their data-stewardship dataset.</summary>
public class DataStewardshipFilterDto
{

    /// <summary>
    /// Dataset identifiers to include.
    /// </summary>
    public ICollection<string> DatasetIdentifiers { get; set; }

    /// <summary>
    /// Data stewardship dataset-title wildcard filter.
    /// </summary>
    public string? DatasetTitleFilter { get; set; }

}


