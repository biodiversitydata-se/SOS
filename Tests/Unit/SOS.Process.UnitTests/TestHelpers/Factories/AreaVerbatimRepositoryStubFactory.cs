using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.UnitTests.TestHelpers.Factories
{
    public static class AreaVerbatimRepositoryStubFactory
    {
        public static Mock<IAreaVerbatimRepository> Create(List<Area> areas)
        {
            Mock<IAreaVerbatimRepository> areaVerbatimRepositoryStub = new Mock<IAreaVerbatimRepository>();
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
    }
}