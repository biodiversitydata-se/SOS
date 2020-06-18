namespace SOS.Lib.Configuration.Export
{
    /// <summary>
    ///     Root config
    /// </summary>
    public class FileDestination
    {
        /// <summary>
        ///     File path
        /// </summary>
        public string Path { get; set; }
    }

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
    }
}