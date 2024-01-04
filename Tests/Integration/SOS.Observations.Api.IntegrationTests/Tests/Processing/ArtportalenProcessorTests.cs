using FizzWare.NBuilder;
using LinqStatistics;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using System.ComponentModel;

namespace SOS.Observations.Api.IntegrationTests.Tests.Processing;

[Collection(TestCollection.Name)]
public class ArtportalenProcessorTests : TestBase
{
    public ArtportalenProcessorTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    [Description("Observation is positive (present) when both NotPresent and NotRecovered is false")]
    public void Observation_is_positive_when_both_NotPresent_and_NotRecovered_is_false()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(60).With(v => v.NotPresent = false)
                         .With(v => v.NotRecovered = false)
             .TheNext(20).With(v => v.NotPresent = true)
             .TheNext(20).With(v => v.NotRecovered = true)
            .Build();

        // Act
        var processedObservations = ProcessFixture.ProcessObservations(verbatimObservations);

        // Assert
        processedObservations
            .Select(m => m.Occurrence.IsPositiveObservation)
            .CountEach()
            .OrderByDescending(m => m.Count)
            .ToList()
            .Should().BeEquivalentTo(
                new List<ItemCount<bool>> {
                    new ItemCount<bool>(true, 60),
                    new ItemCount<bool>(false, 40) });
    }
}