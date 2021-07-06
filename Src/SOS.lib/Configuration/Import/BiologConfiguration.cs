
namespace SOS.Lib.Configuration.Import
{
    /// <summary>
    /// Configuration for bio log
    /// </summary>
    public class BiologConfiguration
    {
        /// <summary>
        /// Authentication token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Url to call to get download url for file
        /// </summary>
        public string Url { get; set; }
    }
}
