using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder.Tests
{
    public class DarwinCoreObservationBuilderTests
    {
        [Fact]
        [Trait("Category", "UnitTest")]
        public void CreateDarwinCoreTestDataWithAllRandomValues()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------           
            const int NrObservations = 100;
            var verbatims = Builder<DwcObservationVerbatim>.CreateListOfSize(NrObservations)
                .All()
                    .HaveRandomValues()
                .TheFirst(1)
                    .With(m => m.CatalogNumber = "123456")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            verbatims.Count.Should().Be(NrObservations);
            verbatims.First().CatalogNumber.Should().Be("123456");
            verbatims.Select(m => m.CatalogNumber).Should().OnlyHaveUniqueItems("because all CatalogNumbers should be unique");
        }

        [Fact]
        [Trait("Category", "UnitTest")]
        public void CreateDarwinCoreTestDataWithValuesFromPredefinedObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const int NrObservations = 10;
            var verbatims = Builder<DwcObservationVerbatim>.CreateListOfSize(NrObservations)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(1)
                    .With(m => m.CatalogNumber = "123456")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            verbatims.Count.Should().Be(NrObservations);
            verbatims.First().CatalogNumber.Should().Be("123456");
            verbatims.Select(m => m.CatalogNumber).Should().OnlyHaveUniqueItems("because all SightingIds should be unique");
        }
    }
}
