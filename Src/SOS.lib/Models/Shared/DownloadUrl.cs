namespace SOS.Lib.Models.Shared
{
    /// <summary>
    /// Download url class
    /// </summary>
    public class DownloadUrl
    {
        public enum DownloadType
        {
            /// <summary>
            /// Url to observations file
            /// </summary>
            Observations,

            /// <summary>
            /// Url to observations eml file
            /// </summary>
            ObservationEml,

            /// <summary>
            /// Url to check list file
            /// </summary>
            CheckLists,

            /// <summary>
            /// Url to check list eml file
            /// </summary>
            CheckListEml

        }

        /// <summary>
        /// Type of url
        /// </summary>
        public DownloadType Type { get; set; }

        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; set; }
    }
}