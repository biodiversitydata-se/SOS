using FluentAssertions;
using SOS.Lib.Helpers;
using System.Linq;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers;

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

    [Fact]
    [Trait("Category", "Unit")]
    public void GetExportFieldsFromOutputFields_ShouldRemoveDuplicates_WhenThereExistsDuplicates()
    {
        // Arrange
        var properties = ObservationPropertyFieldDescriptionHelper.AllFields.Select(m => m.PropertyPath);

        // Act
        var propertiesWithDuplicates = ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(properties, false);
        var propertiesWithoutDuplicates = ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(properties, true);

        // Assert
        propertiesWithoutDuplicates.Select(m => m.SwedishTitle).Distinct().Count().Should().Be(propertiesWithoutDuplicates.Count);
        int nrDuplicates = propertiesWithDuplicates.Count - propertiesWithoutDuplicates.Count;
        nrDuplicates.Should().Be(6, because: "There are 6 potential duplicates");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetExportFieldsFromOutputFields_ShouldRemoveDuplicates_WhenPropertiesIsNull()
    {            
        // Act
        var propertiesWithDuplicates = ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(null, false);
        var propertiesWithoutDuplicates = ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(null, true);

        // Assert
        propertiesWithoutDuplicates.Select(m => m.SwedishTitle).Distinct().Count().Should().Be(propertiesWithoutDuplicates.Count);
        int nrDuplicates = propertiesWithDuplicates.Count - propertiesWithoutDuplicates.Count;
        nrDuplicates.Should().Be(6, because: "There are 6 potential duplicates");
    }
}
