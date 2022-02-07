namespace SOS.Lib.Models.Export
{
    /// <summary>
    /// File export result.
    /// </summary>
    public class FileExportResult
    {
        /// <summary>
        /// Number of observations in the file.
        /// </summary>
        public int NrObservations { get; set; }
        
        /// <summary>
        /// File path.
        /// </summary>
        public string FilePath { get; set; }
    }
}