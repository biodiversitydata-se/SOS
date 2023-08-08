namespace SOS.Lib.Configuration.Export;

/// <summary>
/// DwC-A files creation settings.
/// </summary>
public class DwcaFilesCreationConfiguration
{
    /// <summary>
    /// If true, DwC-A files will be generated in the processing step.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Folder where the DwC-A files will be stored.
    /// </summary>
    public string FolderPath { get; set; }

    /// <summary>
    /// If true, check if the DwC-A files that is created contains any illegal characters.
    /// </summary>
    public bool CheckForIllegalCharacters { get; set; }

    /// <summary>
    /// The minimum number of observations that is needed to create Artportalen DwC-A file.
    /// </summary>
    public int ArtportalenLowerLimitCount { get; set; }
}