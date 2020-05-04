using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

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

            return processedAreaRepositoryStub;
        }

        public static Mock<IProcessedAreaRepository> Create(params AreaType[] areaTypes)
        {
            var areas = AreasTestRepository.LoadAreas(areaTypes);
            return Create(areas.ToList());
        }
    }
}
