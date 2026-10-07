using FizzWare.NBuilder;
using LinqStatistics;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using System.ComponentModel;

namespace SOS.Observations.Api.IntegrationTests.Tests.Processing;

[Collection(TestCollection.Name)]
public class DarwinCoreProcessorTests : TestBase
{
    public DarwinCoreProcessorTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    [Trait("Category", "AutomaticIntegrationTest")]
    [Description("Observation is positive (present) when OccurrenceStatus doesn't have the value 'absent'")]
    public void Observation_is_positive_when_OccurrenceStatus_is_not_null()
    {
        // Arrange
        var verbatimObservations = Builder<DwcObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).With(v => v.OccurrenceStatus = "present")
             .TheNext(1).With(v => v.OccurrenceStatus = "")
             .TheNext(1).With(v => v.OccurrenceStatus = null)
             .TheNext(2).With(v => v.OccurrenceStatus = "absent")
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
                    new ItemCount<bool>(true, 3),
                    new ItemCount<bool>(false, 2) });
    }

    [Fact]
    [Trait("Category", "AutomaticIntegrationTest")]
    [Description("Sensitive species sightings should be protected")]
    public void Sensitive_species_sightings_should_be_protected()
    {
        // Arrange
        var verbatimObservations = Builder<DwcObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(3).HaveSensitiveSpeciesTaxonId()
                         .With(m => m.AccessRights = null)
             .TheNext(1).HaveSensitiveSpeciesTaxonId()
                         .With(m => m.AccessRights = "Free usage") // Sensitive species with "Free usage" => Sensitive=false                
            .Build();

        // Act
        var processedObservations = ProcessFixture.ProcessObservations(verbatimObservations);

        // Assert
        processedObservations
            .Select(m => m.Sensitive)
            .CountEach()
            .OrderByDescending(m => m.Count)
            .ToList()
            .Should().BeEquivalentTo(
                new List<ItemCount<bool>> {
                    new ItemCount<bool>(true, 3),
                    new ItemCount<bool>(false, 2) });
    }
}
