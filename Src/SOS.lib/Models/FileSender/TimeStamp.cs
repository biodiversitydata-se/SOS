namespace SOS.Lib.Models.FileSender
{
    /// <summary>
    /// Time stamp
    /// </summary>
    public class TimeStamp
    {
        /// <summary>
        /// Formatted date
        /// </summary>
        public string Formatted { get; set; }

        /// <summary>
        /// UNIX timestamp
        /// </summary>
        public long Raw { get; set; }
    }
}
