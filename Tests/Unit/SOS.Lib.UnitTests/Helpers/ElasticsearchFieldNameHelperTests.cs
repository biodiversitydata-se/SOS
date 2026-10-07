using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers;

public class ElasticsearchFieldNameHelperTests
{
    [Theory]
    [InlineData("taxon.scientificName", "taxon.scientificName")]
    [InlineData("Taxon.ScientificName", "taxon.scientificName")]
    [InlineData("TAXON.SCIENTIFICNAME", "taxon.scientificName")]
    [InlineData("event.startyear", "event.startYear")]
    [InlineData("EVENT.STARTYEAR", "event.startYear")]
    [InlineData("LOCATION", "location")]
    [InlineData("EVENT.MEASUREMENTORFACTS.MEASUREMENTID", "event.measurementOrFacts.measurementID")]
    [InlineData("Taxon.*", "taxon.*")]
    [InlineData("Unknown.Field", "unknown.field")]
    public void GetFieldName_UsesCanonicalNames(string input, string expected)
    {
        Assert.Equal(expected, ElasticsearchFieldNameHelper.GetFieldName<Observation>(input));
    }

    [Fact]
    public void GetFieldName_UsesTheRequestedEntitysFieldNames()
    {
        Assert.Equal("scientificName", ElasticsearchFieldNameHelper.GetFieldName<Taxon>("SCIENTIFICNAME"));
        Assert.Equal("sCIENTIFICNAME", ElasticsearchFieldNameHelper.GetFieldName<Observation>("SCIENTIFICNAME"));
    }

    [Fact]
    public void GetFieldName_WarmLookup_DoesNotAllocate()
    {
        var fields = new[] { "taxon.scientificName", "TAXON.SCIENTIFICNAME", "taxon.scientificname" };
        for (var i = 0; i < 10_000; i++) ElasticsearchFieldNameHelper.GetFieldName<Observation>(fields[i % fields.Length]);
        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 10_000; i++) ElasticsearchFieldNameHelper.GetFieldName<Observation>(fields[i % fields.Length]);

        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
