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
            /// Url to checklist file
            /// </summary>
            Checklists,

            /// <summary>
            /// Url to checklist eml file
            /// </summary>
            ChecklistEml

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