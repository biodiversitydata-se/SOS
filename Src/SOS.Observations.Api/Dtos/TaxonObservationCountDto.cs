using System;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// Taxon observation count.
    /// </summary>
    public class TaxonObservationCountDto
    {
        /// <summary>
        /// The Taxon Id.
        /// </summary>
        public int TaxonId { get; set; }

        /// <summary>
        /// Observation Count.
        /// </summary>
        [Obsolete("Replaced by ObservationCount")]
        public int Count { get; set; }

        /// <summary>
        /// Observation count.
        /// </summary>
        public int ObservationCount { get; set; }

        /// <summary>
        /// Province count.
        /// </summary>
        public int ProvinceCount { get; set; }
    }
}