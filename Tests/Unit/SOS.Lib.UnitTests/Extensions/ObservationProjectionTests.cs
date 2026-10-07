using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using SOS.Lib.Models.Search.Filters;
using System.Text.Json;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class ObservationProjectionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Projection_UsesCanonicalFields_AndKeepsPrivateFieldsExcluded(bool isInternal)
    {
        var output = new OutputFilter { Fields = ["TAXON.SCIENTIFICNAME", "LOCATION", "Taxon.*"] };

        using var json = Serialize(output.ToProjection(isInternal));

        Assert.Equal(["taxon.scientificName", "location", "taxon.*"],
            json.RootElement.GetProperty("includes").EnumerateArray().Select(field => field.GetString()));
        var excludes = json.RootElement.GetProperty("excludes").EnumerateArray().Select(field => field.GetString()).ToArray();
        Assert.Contains("defects", excludes);
        Assert.Contains("location.attributes.isPrivate", excludes);
        Assert.Contains("location.point", excludes);
        if (isInternal)
        {
            Assert.Contains("artportalenInternal.activityCategoryId", excludes);
            Assert.Contains("artportalenInternal.triggeredObservationRuleUnspontaneous", excludes);
            Assert.DoesNotContain("artportalenInternal", excludes);
        }
        else
        {
            Assert.Contains("artportalenInternal", excludes);
        }
    }

    [Fact]
    public void Projection_DoesNotMutateSharedExclusions_WhenFieldsAreExplicitlyExcluded()
    {
        var output = new OutputFilter { Fields = ["taxon"], ExcludeFields = ["TAXON.SCIENTIFICNAME"] };
        using var specific = Serialize(output.ToProjection(false));
        using var regular = Serialize(new OutputFilter().ToProjection(false));

        Assert.Contains("taxon.scientificName", specific.RootElement.GetProperty("excludes").EnumerateArray().Select(field => field.GetString()));
        Assert.DoesNotContain("taxon.scientificName", regular.RootElement.GetProperty("excludes").EnumerateArray().Select(field => field.GetString()));
    }

    private static JsonDocument Serialize(SourceConfig projection)
    {
        var client = new ElasticsearchClient(new ElasticsearchClientSettings(new Uri("http://localhost:9200")));
        using var stream = new MemoryStream();
        client.RequestResponseSerializer.Serialize(projection, stream);
        return JsonDocument.Parse(stream.ToArray());
    }
}
