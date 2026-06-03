namespace SOS.Lib.Dtos.Filter;

public class DataStewardshipFilterDto
{
    /// <summary>
    /// Dataset filter
    /// </summary>
    public IEnumerable<string>? DatasetIdentifiers { get; set; }

    /// <summary>
    /// DataStewardship title wild card filter
    /// </summary>
    public string? DatasetTitleFilter { get; set; }
}
