namespace SOS.Lib.Models.Shared;

/// <summary>
/// Dataset class
/// </summary>
public class DataProviderDataset
{
    public enum DatasetType
    {
        /// <summary>
        /// Dataset with observations
        /// </summary>
        Observations,

        /// <summary>
        /// Dataset containing check lists
        /// </summary>
        Checklists,
    }

    /// <summary>
    /// Url to data file
    /// </summary>
    public string DataUrl { get; set; }

    /// <summary>
    /// Url to eml file
    /// </summary>
    public string EmlUrl { get; set; }

    /// <summary>
    /// Dataset identifier
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// True if dataset is active
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Name of dataset
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Type of dataset
    /// </summary>
    public DatasetType Type { get; set; }
}