using System.Linq;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class FieldDescriptionHelperTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void AddMandatoryFieldDescriptionIds_When_NotAllMandatoryFieldsIsProvided()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var fieldIds = new[]
            {
                FieldDescriptionId.ScientificName,
                FieldDescriptionId.Continent,
                FieldDescriptionId.AccessRights
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var fields = FieldDescriptionHelper.AddMandatoryFieldDescriptionIds(fieldIds);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            var expectedResult = new[]
            {
                FieldDescriptionId.OccurrenceID,
                FieldDescriptionId.BasisOfRecord,
                FieldDescriptionId.InstitutionCode,
                FieldDescriptionId.CollectionCode,
                FieldDescriptionId.CatalogNumber,
                FieldDescriptionId.ScientificName,
                FieldDescriptionId.DecimalLongitude,
                FieldDescriptionId.DecimalLatitude,
                FieldDescriptionId.CoordinateUncertaintyInMeters,
                FieldDescriptionId.EventDate,
                FieldDescriptionId.Continent,
                FieldDescriptionId.AccessRights
            };

            fields.Should().ContainInOrder(expectedResult,
                "because the mandatory DwC fields are added first in the list");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GetFieldDescriptions_When_NotAllMandatoryFieldsIsProvided()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var userFieldDescriptionIds = new[]
            {
                FieldDescriptionId.ScientificName,
                FieldDescriptionId.Continent,
                FieldDescriptionId.AccessRights
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var fieldDescriptionIds = FieldDescriptionHelper.AddMandatoryFieldDescriptionIds(userFieldDescriptionIds);
            var fieldDescriptions = FieldDescriptionHelper.GetFieldDescriptions(fieldDescriptionIds);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            fieldDescriptions.Should().Contain(x => x.Id == (int) FieldDescriptionId.CatalogNumber);
        }
    }
}