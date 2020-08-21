using System.Collections.Generic;
using System.Linq;
using Moq;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Process.UnitTests.TestHelpers.Factories
{
    public static class ProcessedAreaRepositoryStubFactory
    {
        public static Mock<IProcessedAreaRepository> Create(List<Area> areas)
        {
            var processedAreaRepositoryStub = new Mock<IProcessedAreaRepository>();
            processedAreaRepositoryStub
                .Setup(avm => avm.GetAllAsync())
                .ReturnsAsync(areas);

            processedAreaRepositoryStub
                .Setup(avm => avm.GetAsync(It.IsAny<AreaType[]>()))
                .ReturnsAsync(areas);

            // todo - load geometry for each area and store them in a dictionary.
            IGeoShape geometry = new PointGeoShape(new GeoCoordinate(0,0));
            processedAreaRepositoryStub
                .Setup(avm => avm.GetGeometryAsync(It.IsAny<int>()))
                .ReturnsAsync(geometry);

            return processedAreaRepositoryStub;
        }

        public static Mock<IProcessedAreaRepository> Create(params AreaType[] areaTypes)
        {
            var areas = AreasTestRepository.LoadAreas(areaTypes);
            return Create(areas.ToList());
        }
    }
}