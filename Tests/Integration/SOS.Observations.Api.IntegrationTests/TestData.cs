using System;
using System.Collections.Generic;
using System.Text;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.Observations.Api.IntegrationTests
{
    public static class TestData
    {
        public static class Areas
        {
            /// <summary>
            /// Tranås municipality.
            /// </summary>
            public static AreaFilterDto TranasMunicipality => new AreaFilterDto { Type = AreaType.Municipality, FeatureId = "687" };
            
            /// <summary>
            /// Jönköping county.
            /// </summary>
            public static AreaFilterDto JonkopingCounty => new AreaFilterDto { Type = AreaType.County, FeatureId = "6" };
        }
    }

    
}
