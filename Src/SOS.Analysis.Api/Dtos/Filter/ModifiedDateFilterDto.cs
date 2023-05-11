using System;

namespace SOS.Analysis.Api.Dtos.Filter
{
    /// <summary>
    /// Filter for observation Modified date
    /// </summary>
    public class ModifiedDateFilterDto
    {
        /// <summary>
        /// Changed from
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// Changed tp
        /// </summary>
        public DateTime? To { get; set; }
    }
}
