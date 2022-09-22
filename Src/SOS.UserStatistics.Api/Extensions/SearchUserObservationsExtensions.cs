using SOS.Lib;

public static class SearchUserObservationsExtensions
{
    public static ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> ToQuery<TQueryContainer>(
        this SpeciesCountUserStatisticsQuery filter) where TQueryContainer : class
    {
        var query = new List<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>>();

        if (filter == null)
        {
            return query;
        }
        
        query.TryAddTermCriteria("year", filter.Year);
        query.TryAddTermCriteria("taxonId", filter.TaxonId);
        query.TryAddTermCriteria("speciesGroup", filter.SpeciesGroup);
        if (filter.AreaType.HasValue)
        {
            switch (filter.AreaType.Value)
            {
                case AreaType.Province:
                    query.TryAddTermCriteria("provinceFeatureId", filter.FeatureId);
                    break;
                case AreaType.Municipality:
                    query.TryAddTermCriteria("municipalityFeatureId", filter.FeatureId);
                    break;
                case AreaType.CountryRegion:
                    query.TryAddTermCriteria("countryRegionFeatureId", filter.FeatureId);
                    break;
            }
        }
        return query;
    }


    public static ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> ToProcessedObservationQuery<TQueryContainer>(
        this SpeciesCountUserStatisticsQuery filter) where TQueryContainer : class
    {
        var query = new List<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>>();
       
        if (filter == null)
        {
            return query;
        }

        query.TryAddTermCriteria("event.startYear", filter.Year); // todo
        query.TryAddTermCriteria("taxon.id", filter.TaxonId);
        query.TryAddTermCriteria("taxon.attributes.speciesGroup", filter.SpeciesGroup); // todo
        query.AddSightingTypeFilters(SearchFilterBase.SightingTypeFilter.DoNotShowMerged);

        if (filter.AreaType.HasValue)
        {
            switch (filter.AreaType.Value)
            {
                case AreaType.Province:
                    query.TryAddTermCriteria("location.province.featureId", filter.FeatureId);
                    break;
                case AreaType.Municipality:
                    query.TryAddTermCriteria("location.municipality.featureId", filter.FeatureId);
                    break;
                case AreaType.CountryRegion:
                    query.TryAddTermCriteria("location.countryRegion.featureId", filter.FeatureId); // todo
                    break;
            }
        }

        return query;
    }
}
