using SOS.Lib.Enums;
using SOS.Lib.Models.UserService;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.Observations.Api.IntegrationTests
{
    public static class TestData
    {
        public static class TaxonIds
        {
            public static readonly int Biota = 0;
            public static readonly int Otter = 100077;
            public static readonly int Mammalia = 4000107;
            public static readonly int Wolf = 267320;
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

            /// <summary>
            /// Östergötland county.
            /// </summary>
            public static AreaFilterDto OstergotlandCounty => new AreaFilterDto { AreaType = AreaTypeDto.County, FeatureId = "5" };
        }

        public static class AreaAuthority
        {
            public static AreaModel Sweden => new AreaModel {AreaTypeId = (int) AreaType.BirdValidationArea, FeatureId = "100"};
            public static AreaModel JonkopingCounty => new AreaModel { AreaTypeId = (int)AreaType.County, FeatureId = "6" };
            public static AreaModel OstergotlandCounty => new AreaModel { AreaTypeId = (int)AreaType.County, FeatureId = "5" };
            public static AreaModel TranasMunicipality => new AreaModel { AreaTypeId = (int)AreaType.Municipality, FeatureId = "687" };
        }
    }
}
