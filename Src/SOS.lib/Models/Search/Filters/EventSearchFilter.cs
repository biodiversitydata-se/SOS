using System.Collections.Generic;
using System.Text.Json;

namespace SOS.Lib.Models.Search.Filters
{
    /// <summary>
    ///     Base filter class
    /// </summary>
    public class EventSearchFilter
    {       
        public List<string> DatasetIds { get; set; }
        public bool? IsPartOfDataStewardshipDataset { get; set; }
        public List<string> EventIds { get; set; }

        /// <summary>
        ///     Only get data from these providers
        /// </summary>
        public List<int> DataProviderIds { get; set; }

        public List<SortOrderFilter> SortOrders { get; set; }
        public List<string> OutputIncludeFields { get; set; }
        public List<string> OutputExcludeFields { get; set; }
        // Todo - the commented filters could possibly be added later.

        ///// <summary>
        /////     Which type of date filtering that should be used
        ///// </summary>
        //public DateFilter Date { get; set; }

        ///// <summary>
        /////     Vocabulary mapping translation culture code.
        /////     Available values.
        /////     sv-SE (Swedish)
        /////     en-GB (English)
        ///// </summary>
        //public string FieldTranslationCultureCode { get; set; }

        ///// <summary>
        ///// Location related filter
        ///// </summary>
        //public LocationFilter Location { get; set; }

        public EventSearchFilter Clone()
        {
            var searchFilter = (EventSearchFilter)MemberwiseClone();
            return searchFilter;
        }

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