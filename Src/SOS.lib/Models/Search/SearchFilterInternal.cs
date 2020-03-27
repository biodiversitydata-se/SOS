using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Search filter for the internal advanced search
    /// </summary>
    public class SearchFilterInternal : SearchFilter
    {        
        public int? UserId { get; set; }
        public int? ProjectId { get; set; }     
    }
}
