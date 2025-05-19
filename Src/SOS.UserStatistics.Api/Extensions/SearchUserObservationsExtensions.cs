using Elastic.Clients.Elasticsearch.QueryDsl;
using SOS.Lib;

public static class SearchUserObservationsExtensions
{
    public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> ToQuery<TQueryDescriptor>(
           this SpeciesCountUserStatisticsQuery filter) where TQueryDescriptor : class
    {
        var queries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();

        if (filter == null)
        {
            return queries;
        }

        queries.TryAddTermCriteria("year", filter.Year);
        queries.TryAddTermCriteria("taxonId", filter.TaxonId);
        queries.TryAddTermCriteria("speciesGroup", filter.SpeciesGroup);
        if (filter.AreaType.HasValue)
        {
            switch (filter.AreaType.Value)
            {
                case AreaType.Province:
                    queries.TryAddTermCriteria("provinceFeatureId", filter.FeatureId);
                    break;
                case AreaType.Municipality:
                    queries.TryAddTermCriteria("municipalityFeatureId", filter.FeatureId);
                    break;
                case AreaType.CountryRegion:
                    queries.TryAddTermCriteria("countryRegionFeatureId", filter.FeatureId);
                    break;
            }
        }
        return queries;
    }

    public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> ToProcessedObservationQuery<TQueryDescriptor>(
          this SpeciesCountUserStatisticsQuery filter) where TQueryDescriptor : class
    {
        var queries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();

        if (filter == null)
        {
            return queries;
        }

        queries.TryAddTermCriteria("event.startYear", filter.Year); // todo
        queries.TryAddTermCriteria("taxon.id", filter.TaxonId);
        queries.TryAddTermCriteria("taxon.attributes.speciesGroup", filter.SpeciesGroup); // todo

        queries.AddSightingTypeFilters(SearchFilterBase.SightingTypeFilter.DoNotShowMerged, null);

        if (filter.AreaType.HasValue)
        {
            switch (filter.AreaType.Value)
            {
                case AreaType.Province:
                    queries.TryAddTermCriteria("location.province.featureId", filter.FeatureId);
                    break;
                case AreaType.Municipality:
                    queries.TryAddTermCriteria("location.municipality.featureId", filter.FeatureId);
                    break;
                case AreaType.CountryRegion:
                    queries.TryAddTermCriteria("location.countryRegion.featureId", filter.FeatureId); // todo
                    break;
            }
        }

        return queries;
    }
}
