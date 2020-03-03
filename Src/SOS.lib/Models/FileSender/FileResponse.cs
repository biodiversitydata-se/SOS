namespace SOS.Lib.Models.FileSender
{
    /// <summary>
    /// File sender file
    /// </summary>
    public class FileRequest : File
    {
        /// <summary>
        /// File unique id
        /// </summary>
        public string Cid { get; set; }

        /// <summary>
        /// Mime type of file
        /// </summary>
        public string Mime_type { get; set; }
    }
}
