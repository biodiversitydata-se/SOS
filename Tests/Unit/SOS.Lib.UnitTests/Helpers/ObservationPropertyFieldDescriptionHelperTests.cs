using FluentAssertions;
using SOS.Lib.Helpers;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class ObservationPropertyFieldDescriptionHelperTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void ValidateUniquePropertyNamesAndTitles()
        {
            // Act
            bool uniqueProperties = ObservationPropertyFieldDescriptionHelper.ValidateUniquePropertyNames();
            bool uniqueDependencyMapping = ObservationPropertyFieldDescriptionHelper.ValidateUniqueDependencyMapping();

            // Assert
            uniqueProperties.Should().BeTrue();
            uniqueDependencyMapping.Should().BeTrue();
        }
    }
}
