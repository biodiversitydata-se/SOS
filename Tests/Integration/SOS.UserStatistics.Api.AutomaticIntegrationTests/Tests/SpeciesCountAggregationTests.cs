namespace SOS.UserStatistics.Api.AutomaticIntegrationTests.Tests;

[Collection(Collections.ApiAutomaticIntegrationTestsCollection)]
public class SpeciesCountAggregationTests
{
    private readonly UserStatisticsAutomaticIntegrationTestFixture _fixture;

    public SpeciesCountAggregationTests(UserStatisticsAutomaticIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Test_PagedSpeciesCountAggregation()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(20)
            .All()
                .HaveValuesFromPredefinedObservations()
            .TheFirst(6) // 6 observations, 5 taxa
            .HaveProperties(1,
                    new() { TaxonId = 1 },
                    new() { TaxonId = 1 },
                    new() { TaxonId = 2 },
                    new() { TaxonId = 3 },
                    new() { TaxonId = 4 },
                    new() { TaxonId = 5 })
            .TheNext(4) // 4 observations , 4 taxa
                .HaveProperties(2,
                    new() { TaxonId = 1 },
                    new() { TaxonId = 2 },
                    new() { TaxonId = 3 },
                    new() { TaxonId = 4 })
            .TheNext(5) // 5 observations , 3 taxa
                .HaveProperties(3,
                    new() { TaxonId = 1 },
                    new() { TaxonId = 2 },
                    new() { TaxonId = 2 },
                    new() { TaxonId = 3 },
                    new() { TaxonId = 3 })
            .TheNext(2) // 2 observations , 2 taxa
                .HaveProperties(4,
                    new() { TaxonId = 1 },
                    new() { TaxonId = 2 })
            .TheNext(3) // 3 observations , 1 taxa
                .HaveProperties(5,
                    new() { TaxonId = 1 },
                    new() { TaxonId = 1 },
                    new() { TaxonId = 1 })
            .Build();

        await _fixture.ProcessAndAddUserObservationToElasticSearch(verbatimObservations);
        var query = new SpeciesCountUserStatisticsQuery();

        // Act
        var res = await _fixture.UserStatisticsManager.SpeciesCountSearchAsync(query, 0, 5);

        // Assert
        var expected = new List<UserStatisticsItem>
        {
            new() {UserId = 1, SpeciesCount = 5, ObservationCount = 6},
            new() {UserId = 2, SpeciesCount = 4, ObservationCount = 4},
            new() {UserId = 3, SpeciesCount = 3, ObservationCount = 5},
            new() {UserId = 4, SpeciesCount = 2, ObservationCount = 2},
            new() {UserId = 5, SpeciesCount = 1, ObservationCount = 3}
        };

        res.Records.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Test_SpeciesCountAggregation_with_IncludeOtherAreasSpeciesCount()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(36)
            .All()
                .HaveValuesFromPredefinedObservations()
            .TheFirst(11) // 5 taxa
                .HaveProperties(1,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 5 taxa in P1
                    new() { TaxonId = 1, ProvinceId = "P1" },
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P1" },
                    new() { TaxonId = 4, ProvinceId = "P1" },
                    new() { TaxonId = 5, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P2" }, // 2 taxa in P2
                    new() { TaxonId = 3, ProvinceId = "P2" },
                    new() { TaxonId = 5, ProvinceId = "P2" },
                    new() { TaxonId = 1, ProvinceId = "P3" }, // 2 taxa in P3
                    new() { TaxonId = 4, ProvinceId = "P3" })
            .TheNext(9) // 4 taxa
                .HaveProperties(2,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 4 taxa in P1
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P1" },
                    new() { TaxonId = 4, ProvinceId = "P1" },
                    new() { TaxonId = 1, ProvinceId = "P2" }, // 3 taxa in P2
                    new() { TaxonId = 2, ProvinceId = "P2" },
                    new() { TaxonId = 3, ProvinceId = "P2" },
                    new() { TaxonId = 1, ProvinceId = "P3" }, // 2 taxa in P3
                    new() { TaxonId = 4, ProvinceId = "P3" })
            .TheNext(8) // 3 taxa
                .HaveProperties(3,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 3 taxa in P1
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P2" }, // 1 taxa in P2
                    new() { TaxonId = 3, ProvinceId = "P3" }, // 1 taxa in P3
                    new() { TaxonId = 3, ProvinceId = "P3" })
            .TheNext(4) // 2 taxa
                .HaveProperties(4,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 2 taxa in P1
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 1, ProvinceId = "P2" }, // 2 taxa in P2
                    new() { TaxonId = 2, ProvinceId = "P2" })
            .TheNext(4) // 1 taxa
                .HaveProperties(5,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 1 taxa in P1
                    new() { TaxonId = 1, ProvinceId = "P1" },
                    new() { TaxonId = 1, ProvinceId = "P1" },
                    new() { TaxonId = 1, ProvinceId = "P4" }) // 1 taxa in P4
            .Build();

        await _fixture.ProcessAndAddUserObservationToElasticSearch(verbatimObservations);
        var query = new SpeciesCountUserStatisticsQuery
        {
            AreaType = AreaType.Province,
            IncludeOtherAreasSpeciesCount = true
        };

        // Act
        var res = await _fixture.UserStatisticsManager.SpeciesCountSearchAsync(
            query,
            0,
            5);

        // Assert
        var expected = new List<UserStatisticsItem>
        {
            new() { UserId = 1, SpeciesCount = 5, ObservationCount = 11, SpeciesCountByFeatureId = new Dictionary<string, int> { {"P1", 5}, {"P2", 2}, {"P3", 2} }},
            new() { UserId = 2, SpeciesCount = 4, ObservationCount = 9, SpeciesCountByFeatureId = new Dictionary<string, int>  { {"P1", 4}, {"P2", 3}, {"P3", 2} }},
            new() { UserId = 3, SpeciesCount = 3, ObservationCount = 8, SpeciesCountByFeatureId = new Dictionary<string, int>  { {"P1", 3}, {"P2", 1}, {"P3", 1} }},
            new() { UserId = 4, SpeciesCount = 2, ObservationCount = 4, SpeciesCountByFeatureId = new Dictionary<string, int>  { {"P1", 2}, {"P2", 2} }},
            new() { UserId = 5, SpeciesCount = 1, ObservationCount = 4, SpeciesCountByFeatureId = new Dictionary<string, int>  { {"P1", 1}, {"P4", 1} }},
        };

        res.Records.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Test_SpeciesCountAggregation_sort_by_specific_area()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(36)
            .All()
                .HaveValuesFromPredefinedObservations()
            .TheFirst(11) // 5 taxa
                .HaveProperties(1,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 5 taxa in P1
                    new() { TaxonId = 1, ProvinceId = "P1" },
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P1" },
                    new() { TaxonId = 4, ProvinceId = "P1" },
                    new() { TaxonId = 5, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P2" }, // 2 taxa in P2
                    new() { TaxonId = 3, ProvinceId = "P2" },
                    new() { TaxonId = 5, ProvinceId = "P2" },
                    new() { TaxonId = 1, ProvinceId = "P3" }, // 2 taxa in P3
                    new() { TaxonId = 4, ProvinceId = "P3" })
            .TheNext(9) // 4 taxa
                .HaveProperties(2,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 4 taxa in P1
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P1" },
                    new() { TaxonId = 4, ProvinceId = "P1" },
                    new() { TaxonId = 1, ProvinceId = "P2" }, // 3 taxa in P2
                    new() { TaxonId = 2, ProvinceId = "P2" },
                    new() { TaxonId = 3, ProvinceId = "P2" },
                    new() { TaxonId = 1, ProvinceId = "P3" }, // 2 taxa in P3
                    new() { TaxonId = 4, ProvinceId = "P3" })
            .TheNext(8) // 3 taxa
                .HaveProperties(3,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 3 taxa in P1
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P2" }, // 1 taxa in P2
                    new() { TaxonId = 3, ProvinceId = "P3" }, // 1 taxa in P3
                    new() { TaxonId = 3, ProvinceId = "P3" })
            .TheNext(4) // 2 taxa
                .HaveProperties(4,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 2 taxa in P1
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 1, ProvinceId = "P2" }, // 2 taxa in P2
                    new() { TaxonId = 2, ProvinceId = "P2" })
            .TheNext(4) // 1 taxa
                .HaveProperties(5,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 1 taxa in P1
                    new() { TaxonId = 1, ProvinceId = "P1" },
                    new() { TaxonId = 1, ProvinceId = "P1" },
                    new() { TaxonId = 1, ProvinceId = "P4" }) // 1 taxa in P4
            .Build();

        await _fixture.ProcessAndAddUserObservationToElasticSearch(verbatimObservations);

        // Act
        var res = await _fixture.UserStatisticsManager.SpeciesCountSearchAsync(
            new SpeciesCountUserStatisticsQuery
            {
                AreaType = AreaType.Province,
                IncludeOtherAreasSpeciesCount = true,
                SortByFeatureId = "P2"
            },
            0,
            5);

        // Assert
        var expected = new List<UserStatisticsItem>
        {
            new() { UserId = 2, SpeciesCount = 4, ObservationCount = 9, SpeciesCountByFeatureId = new Dictionary<string, int>  { {"P1", 4}, {"P2", 3}, {"P3", 2} }},
            new() { UserId = 1, SpeciesCount = 5, ObservationCount = 11, SpeciesCountByFeatureId = new Dictionary<string, int> { {"P1", 5}, {"P2", 2}, {"P3", 2} }},
            new() { UserId = 4, SpeciesCount = 2, ObservationCount = 4, SpeciesCountByFeatureId = new Dictionary<string, int>  { {"P1", 2}, {"P2", 2} }},
            new() { UserId = 3, SpeciesCount = 3, ObservationCount = 8, SpeciesCountByFeatureId = new Dictionary<string, int>  { {"P1", 3}, {"P2", 1}, {"P3", 1} }},
            //new() { UserId = 5, SpeciesCount = 1, ObservationCount = 4 }, // This user doesn't have any observations in P2, so it is excluded in the result
        };
        res.Records.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Test_SpeciesCountAggregation_filter_by_specific_area()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(36)
            .All()
                .HaveValuesFromPredefinedObservations()
            .TheFirst(11) // 5 taxa
                .HaveProperties(1,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 5 taxa in P1
                    new() { TaxonId = 1, ProvinceId = "P1" },
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P1" },
                    new() { TaxonId = 4, ProvinceId = "P1" },
                    new() { TaxonId = 5, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P2" }, // 2 taxa in P2
                    new() { TaxonId = 3, ProvinceId = "P2" },
                    new() { TaxonId = 5, ProvinceId = "P2" },
                    new() { TaxonId = 1, ProvinceId = "P3" }, // 2 taxa in P3
                    new() { TaxonId = 4, ProvinceId = "P3" })
            .TheNext(9) // 4 taxa
                .HaveProperties(2,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 4 taxa in P1
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P1" },
                    new() { TaxonId = 4, ProvinceId = "P1" },
                    new() { TaxonId = 1, ProvinceId = "P2" }, // 3 taxa in P2
                    new() { TaxonId = 2, ProvinceId = "P2" },
                    new() { TaxonId = 3, ProvinceId = "P2" },
                    new() { TaxonId = 1, ProvinceId = "P3" }, // 2 taxa in P3
                    new() { TaxonId = 4, ProvinceId = "P3" })
            .TheNext(8) // 3 taxa
                .HaveProperties(3,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 3 taxa in P1
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P1" },
                    new() { TaxonId = 3, ProvinceId = "P2" }, // 1 taxa in P2
                    new() { TaxonId = 3, ProvinceId = "P3" }, // 1 taxa in P3
                    new() { TaxonId = 3, ProvinceId = "P3" })
            .TheNext(4) // 2 taxa
                .HaveProperties(4,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 2 taxa in P1
                    new() { TaxonId = 2, ProvinceId = "P1" },
                    new() { TaxonId = 1, ProvinceId = "P2" }, // 2 taxa in P2
                    new() { TaxonId = 2, ProvinceId = "P2" })
            .TheNext(4) // 1 taxa
                .HaveProperties(5,
                    new() { TaxonId = 1, ProvinceId = "P1" }, // 1 taxa in P1
                    new() { TaxonId = 1, ProvinceId = "P1" },
                    new() { TaxonId = 1, ProvinceId = "P1" },
                    new() { TaxonId = 1, ProvinceId = "P4" }) // 1 taxa in P4
            .Build();

        await _fixture.ProcessAndAddUserObservationToElasticSearch(verbatimObservations);

        // Act
        var res = await _fixture.UserStatisticsManager.SpeciesCountSearchAsync(
            new SpeciesCountUserStatisticsQuery
            {
                AreaType = AreaType.Province,
                FeatureId = "P2"
            },
            0,
            5);

        // Assert
        var expected = new List<UserStatisticsItem>
        {
            new() { UserId = 2, SpeciesCount = 3, ObservationCount = 3 },
            new() { UserId = 1, SpeciesCount = 2, ObservationCount = 3 },
            new() { UserId = 4, SpeciesCount = 2, ObservationCount = 2 },
            new() { UserId = 3, SpeciesCount = 1, ObservationCount = 1 }
            //new() { UserId = 5, SpeciesCount = 0, ObservationCount = 0 } // This user doesn't have any observations in P2, so it is excluded in the result
        };
        res.Records.Should().BeEquivalentTo(expected);
    }

    [Fact(Skip = "Run on demand because the test takes about 1 minute to run")]
    public async Task Test_SpeciesCount_with_large_amount_of_data()
    {
        // Arrange
        const int nrUsers = 100000; // When using composite aggregation we can handle more than 65536 users.
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(nrUsers)
            .All()
                .HaveValuesFromPredefinedObservations()
            .Build();

        for (var i = 0; i < nrUsers; i++)
        {
            var obs = verbatimObservations[i];
            int userId = i;
            obs.ObserversInternal = new List<UserInternal> { new() { Id = userId, UserServiceUserId = userId } };
            obs.TaxonId = 1;
        }

        var verbatimObservations2 = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(15)
            .All()
                .HaveValuesFromPredefinedObservations()
            .TheFirst(6) // 6 observations, 5 taxa
                .HaveProperties(nrUsers,
                    new() { TaxonId = 1 },
                    new() { TaxonId = 1 },
                    new() { TaxonId = 2 },
                    new() { TaxonId = 3 },
                    new() { TaxonId = 4 },
                    new() { TaxonId = 5 })
            .TheNext(4) // 4 observations , 4 taxa
                .HaveProperties(nrUsers + 1,
                    new() { TaxonId = 1 },
                    new() { TaxonId = 2 },
                    new() { TaxonId = 3 },
                    new() { TaxonId = 4 })
            .TheNext(5) // 5 observations , 3 taxa
                .HaveProperties(nrUsers + 2,
                    new() { TaxonId = 1 },
                    new() { TaxonId = 2 },
                    new() { TaxonId = 2 },
                    new() { TaxonId = 3 },
                    new() { TaxonId = 3 })
            .Build();

        await _fixture.ProcessAndAddUserObservationToElasticSearch(verbatimObservations.Union(verbatimObservations2));
        Thread.Sleep(5000);
        var query = new SpeciesCountUserStatisticsQuery();

        // Act
        var res = await _fixture.UserStatisticsManager.SpeciesCountSearchAsync(query, 0, 5, useCache: true);

        // Assert
        var expected = new List<UserStatisticsItem>
        {
            new() {UserId = nrUsers, SpeciesCount = 5, ObservationCount = 6},
            new() {UserId = nrUsers+1, SpeciesCount = 4, ObservationCount = 4},
            new() {UserId = nrUsers+2, SpeciesCount = 3, ObservationCount = 5},
            new() {UserId = 0, SpeciesCount = 1, ObservationCount = 1},
            new() {UserId = 1, SpeciesCount = 1, ObservationCount = 1}
        };

        res.Records.Should().BeEquivalentTo(expected);
    }
}