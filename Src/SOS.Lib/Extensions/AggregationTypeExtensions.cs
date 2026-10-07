using SOS.Api.Contracts.Aggregations.Requests;

namespace SOS.Lib.Extensions;

/// <summary>Classifies the aggregation requested by an API caller.</summary>
public static class AggregationTypeExtensions
{
    extension(AggregationType aggregationType)
    {
        public bool IsDateHistogram() => aggregationType is
            AggregationType.QuantityPerWeek or AggregationType.QuantityPerYear or
            AggregationType.SightingsPerWeek or AggregationType.SightingsPerYear;

        public bool IsYearHistogram() => aggregationType is
            AggregationType.QuantityPerYear or AggregationType.SightingsPerYear;

        public bool IsWeekHistogram() => aggregationType is
            AggregationType.QuantityPerWeek or AggregationType.SightingsPerWeek or
            AggregationType.SightingsPerWeek48;

        public bool IsSpeciesSightingsList() => aggregationType is
            AggregationType.SpeciesSightingsList or AggregationType.SpeciesSightingsListTaxonCount;
    }
}
