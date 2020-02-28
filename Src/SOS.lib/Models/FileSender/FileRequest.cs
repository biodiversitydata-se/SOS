namespace SOS.Lib.Models.FileSender
{
    /// <summary>
    /// File sender file
    /// </summary>
    public class FileResponse : File
    {
        /// <summary>
        /// File unique id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// File hash
        /// </summary>
        public string Sha1 { get; set; }

        /// <summary>
        /// Uid
        /// </summary>
        public string Uid { get; set; }
    }
}
