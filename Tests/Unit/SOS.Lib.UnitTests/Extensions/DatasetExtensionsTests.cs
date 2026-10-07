using FluentAssertions;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Lib.Extensions.Dto.DataStewardship;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.DataStewardship.Enums;
using System;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class DatasetExtensionsTests
{
    [Fact]
    public void DatasetToDto_PreservesAllEnumValues()
    {
        foreach (var value in Enum.GetValues<AccessRights>())
            new Dataset { AccessRights = value }.ToDto().AccessRights.Should().Be((DsAccessRights)value);

        foreach (var value in Enum.GetValues<ProgrammeArea>())
            new Dataset { ProgrammeArea = value }.ToDto().ProgrammeArea.Should().Be((DsProgrammeArea)value);

        foreach (var value in Enum.GetValues<Purpose>())
            new Dataset { Purpose = value }.ToDto().Purpose.Should().Be((DsPurpose)value);
    }

    [Fact]
    public void DatasetToDto_PreservesNullEnums()
    {
        var dto = new Dataset().ToDto();

        dto.AccessRights.Should().BeNull();
        dto.ProgrammeArea.Should().BeNull();
        dto.Purpose.Should().BeNull();
    }

    [Fact]
    public void ToProgrammeArea_can_parse_existing_enum_value()
    {
        // Arrange
        var apProgrammeArea = new ArtportalenDatasetMetadata.ProgrammeArea
        {
            Id = (int)Lib.Models.Processed.DataStewardship.Enums.ProgrammeArea.Fjäll
        };

        // Act
        var programArea = apProgrammeArea.ToProgrammeArea();

        // Assert
        programArea.Should().Be(Lib.Models.Processed.DataStewardship.Enums.ProgrammeArea.Fjäll);
    }

    [Fact]
    public void ToProgrammeArea_throws_exception_given_non_existing_enum_value()
    {
        // Arrange
        var apProgrammeArea = new ArtportalenDatasetMetadata.ProgrammeArea
        {
            Id = 1000
        };

        // Act
        Action act = () => apProgrammeArea.ToProgrammeArea();

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
