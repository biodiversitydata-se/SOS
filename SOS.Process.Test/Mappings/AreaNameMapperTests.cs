using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using FluentAssertions;
using SOS.Process.Mappings;
using Xunit;

namespace SOS.Process.Test.Mappings
{
    public class AreaNameMapperTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void TestLoadAreaMappings()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange, Act
            //-----------------------------------------------------------------------------------------------------------
            var areaNameMapper = new AreaNameMapper();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            areaNameMapper.CountyNameByCountyNameSpelling.Should().NotBeEmpty();
            areaNameMapper.ProvinceNameByProvinceNameSpelling.Should().NotBeEmpty();
            areaNameMapper.ProvinceNameByProvinceNameSpelling
                .Should().ContainKey("Hallnad")
                .WhichValue.Should().Be("Halland");
        }
    }
}
