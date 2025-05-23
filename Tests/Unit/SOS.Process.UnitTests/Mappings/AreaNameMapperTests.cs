﻿using FluentAssertions;
using SOS.Harvest.Mappings;
using Xunit;

namespace SOS.Process.UnitTests.Mappings
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
                .WhoseValue.Should().Be("Halland");
        }
    }
}