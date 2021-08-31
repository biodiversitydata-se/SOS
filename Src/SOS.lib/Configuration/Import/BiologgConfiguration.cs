
namespace SOS.Lib.Configuration.Import
{
    /// <summary>
    /// Configuration for Biologg
    /// </summary>
    public class BiologgConfiguration
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
