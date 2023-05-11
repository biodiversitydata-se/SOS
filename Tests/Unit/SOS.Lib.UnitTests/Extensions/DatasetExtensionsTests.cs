using FluentAssertions;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using System;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions
{
    public class DatasetExtensionsTests
    {
        [Fact]
        public void ToProgrammeArea_can_parse_existing_enum_value()
        {
            // Arrange
            var apProgrammeArea = new ArtportalenDatasetMetadata.ProgrammeArea {
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
            var apProgrammeArea = new ArtportalenDatasetMetadata.ProgrammeArea {
                Id = 1000
            };

            // Act
            Action act = () => apProgrammeArea.ToProgrammeArea();

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}