using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Configuration.Import
{
    public class AreaHarvestConfiguration
    {
        /// <summary>
        /// If true, area harvest uses GeoRegion API; otherwise use Artportalen harvest.
        /// </summary>
        public bool UseGeoRegionApiHarvest { get; set; }
    }
}