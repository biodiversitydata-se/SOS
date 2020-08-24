using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.UnitTests.TestHelpers.Factories
{
    public static class AreaVerbatimRepositoryStubFactory
    {
        public static Mock<IAreaVerbatimRepository> Create(List<AreaWithGeometry> areasWithGeometry)
        {
            var areas = GetAreas(areasWithGeometry);
            var areaVerbatimRepositoryStub = new Mock<IAreaVerbatimRepository>();
            areaVerbatimRepositoryStub
                .Setup(avm => avm.GetAllAsync())
                .ReturnsAsync(areas);

            return areaVerbatimRepositoryStub;
        }

        public static Mock<IAreaVerbatimRepository> Create(params AreaType[] areaTypes)
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