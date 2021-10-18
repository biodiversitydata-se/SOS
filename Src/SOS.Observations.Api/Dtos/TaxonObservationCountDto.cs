using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Extensions;

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
        /// Count.
        /// </summary>
        public int Count { get; set; }
    }
}