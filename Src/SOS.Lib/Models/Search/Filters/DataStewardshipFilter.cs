namespace SOS.Lib.Models.Search.Filters;

public class DataStewardshipFilter
{
    /// <summary>
    /// Dataset filter
    /// </summary>
    public IEnumerable<string> DatasetIdentifiers { get; set; }

    /// <summary>
    /// DataStewardship title wild card filter
    /// </summary>
    public string DatasetTitleFilter { get; set; }
}
