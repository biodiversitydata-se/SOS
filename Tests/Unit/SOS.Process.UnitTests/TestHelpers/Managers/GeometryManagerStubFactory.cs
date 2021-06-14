using Moq;
using NetTopologySuite.Geometries;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Process.UnitTests.TestHelpers.Managers
{
    public static class GeometryManagerStubFactory
    {
        public static Mock<IGeometryManager> Create()
        {
            var geometryManagerMock = new Mock<IGeometryManager>();
            geometryManagerMock.Setup(g => g.GetCircleAsync(It.IsAny<Point>(), It.IsAny<int?>()))
                .ReturnsAsync(null as Geometry);
            return geometryManagerMock;
        }
    }
}