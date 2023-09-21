using LinqStatistics;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;

namespace SOS.Observations.Api.IntegrationTests.Helpers.DataUtils;

[Collection(TestCollection.Name)]
public class CalculateArtportalenDataStatistics : TestBase
{
    public CalculateArtportalenDataStatistics(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact(Skip = "Intended to run on demand when needed")]
    [Trait("Category", "DataUtil")]
    public async Task CalculateArtportalenVerbatimDataStatistics()
    {
        // Read observations from MongoDB
        using var cursor = await TestFixture.HarvestFixture.ArtportalenVerbatimRepository.GetAllByCursorAsync();
        const int NrRowsToRead = 1000;
        int nrRowsRead = 0;
        var verbatimObservations = new List<ArtportalenObservationVerbatim>();
        while (await cursor.MoveNextAsync())
        {
            if (nrRowsRead >= NrRowsToRead) break;
            foreach (var row in cursor.Current)
            {
                if (nrRowsRead >= NrRowsToRead) break;
                verbatimObservations.Add(row);
                nrRowsRead++;
            }
        }

        CalculateStatistics(verbatimObservations);
    }

    private void CalculateStatistics(List<ArtportalenObservationVerbatim> observations)
    {
        var activityStatistics = observations.Select(m => m.Activity?.Id)
            .CountEach()
            .OrderByDescending(m => m.Count)
            .ToList();

        var collectionIdStatistics = observations.Select(m => m.CollectionID)
            .CountEach()
            .OrderByDescending(m => m.Count)
            .ToList();

        // todo - add more statistics and write it to file
    }
}