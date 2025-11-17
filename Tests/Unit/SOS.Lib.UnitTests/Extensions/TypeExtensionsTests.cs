using FluentAssertions;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.TaxonTree;
using System;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class TypeExtensionsTests
{
    [Theory]
    [InlineData(typeof(AreaHelper), "AreaHelper")]
    [InlineData(typeof(TaxonTree<IBasicTaxon>), "TaxonTree<IBasicTaxon>")]
    public void GetFormattedName_ReturnsExpectedTypeName(Type type, string expectedName)
    {
        // Act
        string name = type.GetFormattedName();

        // Assert
        name.Should().Be(expectedName);
    }       
}