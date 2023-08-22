using SOS.Observations.Api.Dtos.Filter;

namespace SOS.ContainerIntegrationTests.TestData.Factories;
internal static class SearchFilterDtoFactory
{
    public static SearchFilterDto CreateWithMunicipalityFeatureIds(params string[] featureIds)
    {        
        return new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto 
            { 
                Areas = featureIds.Select(id => new AreaFilterDto
                {
                    AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Municipality,
                    FeatureId = id
                })
            }
        };
    }

    public static SearchFilterDto CreateWithRedListCategories(params string[] categories)
    {
        return new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                RedListCategories = categories
            }
        };
    }

    public static SearchFilterDto CreateWithDataProviderIds(params int[] dataProviderIds)
    {
        return new SearchFilterDto
        {
            DataProvider = new DataProviderFilterDto
            {
                Ids = dataProviderIds.ToList()
            }
        };
    }

    public static SearchFilterDto CreateWithTaxonIds(params int[] taxonIds)
    {
        return new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                Ids = taxonIds
            }
        };
    }

    public static SearchFilterDto CreateWithTaxonIds(bool includeUnderlyingTaxa = false, params int[] taxonIds)
    {
        return new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                Ids = taxonIds,
                IncludeUnderlyingTaxa = includeUnderlyingTaxa
            }
        };
    }

    public static SearchFilterDto CreateWithTaxonListId(int taxonListId, TaxonListOperatorDto listOperator, params int[] taxonIds)
    {
        return new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                TaxonListIds = new[] { taxonListId },
                Ids = taxonIds,
                TaxonListOperator = listOperator
            }
        };
    }

    public static SearchFilterDto CreateWithTaxonCategories(params int[] taxonCategoryIds)
    {
        return new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                TaxonCategories = taxonCategoryIds
            }
        };
    }
}