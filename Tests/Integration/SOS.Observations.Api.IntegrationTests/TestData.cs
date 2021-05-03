using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.Observations.Api.IntegrationTests
{
    public static class TestData
    {
        public static class TaxonIds
        {
            public static readonly int Otter = 100077;
            public static readonly int Mammalia = 4000107;
            public static readonly int Wolf = 100024;
            public static readonly int Aves = 4000104;
        }

        public static class Areas
        {
            /// <summary>
            /// Tranås municipality.
            /// </summary>
            public static AreaFilterDto TranasMunicipality => new AreaFilterDto { AreaType = AreaTypeDto.Municipality, FeatureId = "687" };
            
            /// <summary>
            /// Jönköping county.
            /// </summary>
            public static AreaFilterDto JonkopingCounty => new AreaFilterDto { AreaType = AreaTypeDto.County, FeatureId = "6" };
        }
    }

    
}
