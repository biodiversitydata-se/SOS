using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.Setup;

[Collection(TestCollection.Name)]
public class TestDataLifecycleTests(TestFixture testFixture, ITestOutputHelper output) : TestBase(testFixture, output)
{
    [Fact]
    public async Task Observations_CanBeReplacedAppendedAndCleared_InBothIndices()
    {
        var verbatim = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        var observations = ProcessFixture.ProcessObservations(verbatim);
        foreach (var observation in observations) observation.Occurrence.SensitivityCategory = 1;
        observations[1].Occurrence.SensitivityCategory = 3;

        await ProcessFixture.AddObservationsToElasticsearchAsync(observations.Take(2));
        (await ProcessFixture.GetObservationsCount()).Should().Be(1);
        (await ProcessFixture.GetObservationsCount(protectedIndex: true)).Should().Be(1);

        await ProcessFixture.AddObservationsToElasticsearchAsync(observations.Take(1));
        (await ProcessFixture.GetObservationsCount()).Should().Be(1);
        (await ProcessFixture.GetObservationsCount(protectedIndex: true)).Should().Be(0);

        await ProcessFixture.AddObservationsToElasticsearchAsync(observations.Skip(1), clearExistingObservations: false);
        (await ProcessFixture.GetObservationsCount()).Should().Be(2);
        (await ProcessFixture.GetObservationsCount(protectedIndex: true)).Should().Be(1);

        await ProcessFixture.AddObservationsToElasticsearchAsync([]);
        (await ProcessFixture.GetObservationsCount()).Should().Be(0);
        (await ProcessFixture.GetObservationsCount(protectedIndex: true)).Should().Be(0);
    }

    [Fact]
    public async Task DatasetsAndEvents_CanBeReplacedAppendedAndCleared()
    {
        var datasets = Enumerable.Range(1, 3).Select(id => new Dataset { Id = $"dataset-{id}", Identifier = $"dataset-{id}" }).ToArray();
        var events = Enumerable.Range(1, 3).Select(id => new Event { Id = $"event-{id}", EventId = $"event-{id}" }).ToArray();

        await ProcessFixture.AddDatasetsToElasticsearchAsync(datasets.Take(2));
        await ProcessFixture.AddEventsToElasticsearchAsync(events.Take(2));
        (await ProcessFixture.GetDatasetsCount()).Should().Be(2);
        (await ProcessFixture.GetEventsCount()).Should().Be(2);

        await ProcessFixture.AddDatasetsToElasticsearchAsync(datasets.Take(1));
        await ProcessFixture.AddEventsToElasticsearchAsync(events.Take(1));
        (await ProcessFixture.GetDatasetsCount()).Should().Be(1);
        (await ProcessFixture.GetEventsCount()).Should().Be(1);

        await ProcessFixture.AddDatasetsToElasticsearchAsync(datasets.Skip(1), clearExistingObservations: false);
        await ProcessFixture.AddEventsToElasticsearchAsync(events.Skip(1), clearExistingObservations: false);
        (await ProcessFixture.GetDatasetsCount()).Should().Be(3);
        (await ProcessFixture.GetEventsCount()).Should().Be(3);

        await ProcessFixture.AddDatasetsToElasticsearchAsync([]);
        await ProcessFixture.AddEventsToElasticsearchAsync([]);
        (await ProcessFixture.GetDatasetsCount()).Should().Be(0);
        (await ProcessFixture.GetEventsCount()).Should().Be(0);
    }

    [Fact]
    public async Task Checklists_AreImmediatelySearchable_AndCanBeReplaced()
    {
        var checklists = Builder<ArtportalenChecklistVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedChecklists()
            .Build();
        var repository = TestFixture.ServiceProvider.GetRequiredService<IProcessedChecklistRepository>();

        await ProcessFixture.ProcessAndAddChecklistsToElasticSearch(checklists);
        (await repository.IndexCountAsync()).Should().Be(3);

        await ProcessFixture.ProcessAndAddChecklistsToElasticSearch(checklists.Take(1));
        (await repository.IndexCountAsync()).Should().Be(1);

        await ProcessFixture.CleanElasticsearchIndices();
        (await repository.IndexCountAsync()).Should().Be(0);
        await ProcessFixture.ProcessAndAddChecklistsToElasticSearch(checklists);
        (await repository.IndexCountAsync()).Should().Be(3);
    }
}
