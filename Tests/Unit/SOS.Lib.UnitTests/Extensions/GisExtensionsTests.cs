using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions
{
    public class GisExtensionsTests
    {
        [Fact]
        public void TransformFromWgs84ToGoogleMercator()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act, Assert
            //-----------------------------------------------------------------------------------------------------------
           var point = new Point(12.92257, 55.77695);

           var transformedPoint = point.Transform(CoordinateSys.WGS84, CoordinateSys.WebMercator);
        }
    }
}