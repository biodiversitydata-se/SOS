using System.Collections.Generic;
using System.Text.Json;

namespace SOS.Lib.Models.Search.Filters
{
    /// <summary>
    /// Search filter used to in checklist search
    /// </summary>
    public class ChecklistSearchFilter
    {
        /// <summary>
        ///     Only get data from these providers
        /// </summary>
        public IEnumerable<int> DataProviderIds { get; set; }

        /// <summary>
        ///     Which type of date filtering that should be used
        /// </summary>
        public ChecklistDateFilter Date { get; set; }

        /// <summary>
        /// Location filter
        /// </summary>
        public LocationFilter Location { get; set; }

        /// <summary>
        /// Project id's to match.
        /// </summary>
        public IEnumerable<int> ProjectIds { get; set; }

        /// <summary>
        ///    Taxon related filter
        /// </summary>
        public TaxonFilter Taxa { get; set; }

        /// <summary>
        /// Convert filter to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}