using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Lib.Extensions;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class AggregationTypeExtensionsTests
{
    [Theory]
    [InlineData(AggregationType.SightingsPerWeek, true, false, true, false)]
    [InlineData(AggregationType.SightingsPerYear, true, true, false, false)]
    [InlineData(AggregationType.QuantityPerWeek, true, false, true, false)]
    [InlineData(AggregationType.QuantityPerYear, true, true, false, false)]
    [InlineData(AggregationType.SpeciesSightingsList, false, false, false, true)]
    [InlineData(AggregationType.SpeciesSightingsListTaxonCount, false, false, false, true)]
    [InlineData(AggregationType.SightingsPerWeek48, false, false, true, false)]
    [InlineData((AggregationType)999, false, false, false, false)]
    public void SharedAggregationType_PreservesClassification(
        AggregationType value, bool dateHistogram, bool yearHistogram, bool weekHistogram, bool speciesList)
    {
        Assert.Equal(dateHistogram, value.IsDateHistogram());
        Assert.Equal(yearHistogram, value.IsYearHistogram());
        Assert.Equal(weekHistogram, value.IsWeekHistogram());
        Assert.Equal(speciesList, value.IsSpeciesSightingsList());
    }
}
