using System.Collections.Generic;

namespace SOS.Lib.Models.Verbatim.Shark
{
    /// <summary>
    ///     Verbatim from Shark
    /// </summary>
    public class SharkObservationResponse
    {
        /// <summary>
        ///     Array of properties in the rows
        /// </summary>
        public IEnumerable<string> Header { get; set; }

        /// <summary>
        ///     Current page
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        ///     Total number of pages
        /// </summary>
        public int Pages { get; set; }

        /// <summary>
        ///     Observations per page
        /// </summary>
        public int Per_page { get; set; }

        /// <summary>
        ///     Data rows
        /// </summary>
        public IEnumerable<IEnumerable<string>> Rows { get; set; }

        /// <summary>
        ///     Total number of observations
        /// </summary>
        public int Total { get; set; }
    }
}