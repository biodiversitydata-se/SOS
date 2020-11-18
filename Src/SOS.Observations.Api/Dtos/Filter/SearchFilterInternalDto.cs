using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Internal search filter.
    /// </summary>
    public class SearchFilterInternalDto : SearchFilterDto
    {
      
        public bool IncludeRealCount { get; set; }

        public ArtportalenFilterDto ArtportalenFilter { get; set; }
    }
}