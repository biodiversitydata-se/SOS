using Elastic.Clients.Elasticsearch;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Lib.Extensions;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class AggregationSortOrderExtensionsTests
{
    [Theory]
    [InlineData(AggregationSortOrder.CountDescending, "_count", SortOrder.Desc)]
    [InlineData(AggregationSortOrder.CountAscending, "_count", SortOrder.Asc)]
    [InlineData(AggregationSortOrder.KeyAscending, "_key", SortOrder.Asc)]
    [InlineData(AggregationSortOrder.KeyDescending, "_key", SortOrder.Desc)]
    [InlineData(AggregationSortOrder.OrganismQuantityAscending, "totalOrganismQuantity", SortOrder.Asc)]
    [InlineData(AggregationSortOrder.OrganismQuantityDescending, "totalOrganismQuantity", SortOrder.Desc)]
    public void GetTermsOrder_PreservesFieldAndDirection(AggregationSortOrder value, string field, SortOrder direction)
    {
        var order = Assert.Single(value.GetTermsOrder());

        Assert.Equal(Field.FromString(field), order.Key);
        Assert.Equal(direction, order.Value);
    }
}
