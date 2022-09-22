﻿using System;
using System.Collections.Generic;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Statistics;

namespace SOS.Lib.Extensions;

/// <summary>
/// User observations search extensions.
/// </summary>
public static class SearchExtensionsUserObservations
{
    /// <summary>
    ///     Create search filter
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
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
        query.AddSightingTypeFilters(SearchFilterBase.SightingTypeFilter.DoNotShowMerged);
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

        query.TryAddTermCriteria("year", filter.Year); // todo
        query.TryAddTermCriteria("taxon.id", filter.TaxonId);
        query.TryAddTermCriteria("speciesGroup", filter.SpeciesGroup); // todo
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