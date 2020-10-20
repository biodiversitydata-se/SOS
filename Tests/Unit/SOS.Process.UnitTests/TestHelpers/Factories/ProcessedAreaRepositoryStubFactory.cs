using System.Collections.Generic;
using System.Linq;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Process.UnitTests.TestHelpers.Factories
{
    public static class ProcessedAreaRepositoryStubFactory
    {
        public static Mock<IAreaRepository> Create(List<AreaWithGeometry> areasWithGeometry)
        {
            var areaById = areasWithGeometry.ToDictionary(area => area.Id, area => area);
            var areas = GetAreas(areasWithGeometry);

            var processedAreaRepositoryStub = new Mock<IAreaRepository>();
            processedAreaRepositoryStub
                .Setup(avm => avm.GetAllAsync())
                .ReturnsAsync(areas);

            processedAreaRepositoryStub
                .Setup(avm => avm.GetAsync(It.IsAny<AreaType[]>()))
                .ReturnsAsync(areas);

            processedAreaRepositoryStub
                .Setup(avm => avm.GetGeometryAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) =>
                {
                    if (areaById.TryGetValue(id, out AreaWithGeometry area))
                    {
                        return area.Geometry;
                    }

                    return null;
                });

            return processedAreaRepositoryStub;
        }

        public static Mock<IAreaRepository> Create(params AreaType[] areaTypes)
        {
            var areas = AreasTestRepository.LoadAreas(areaTypes);
            return Create(areas.ToList());
        }

        private static List<Area> GetAreas(List<AreaWithGeometry> areasWithGeometry)
        {
            List<Area> areas = new List<Area>();
            foreach (var areaWithGeometry in areasWithGeometry)
            {
                areas.Add(new Area(areaWithGeometry.AreaType)
                {
                    FeatureId = areaWithGeometry.FeatureId,
                    Id = areaWithGeometry.Id,
                    Name = areaWithGeometry.Name
                });
            }

            return areas;
        }
    }
}