﻿using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.TestData.Factories;

internal static class ArtportalenTestDataFactory
{
    public static IList<ArtportalenObservationVerbatim> Create100RecordsWithRedlistCategories(
        string? category1, string? category2, string? category3, string? category4, string? category5)
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>
            .CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20).HaveRedlistedTaxonId(category1)
             .TheNext(20).HaveRedlistedTaxonId(category2)
             .TheNext(20).HaveRedlistedTaxonId(category3)
             .TheNext(20).HaveRedlistedTaxonId(category4)
             .TheNext(20).HaveRedlistedTaxonId(category5)
            .Build();

        return verbatimObservations;
    }

    public static IList<ArtportalenObservationVerbatim> Create100RecordsWithTaxonIds(int taxonId1, int taxonId2, int taxonId3, int taxonId4, int taxonId5)
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20).With(o => o.TaxonId = taxonId1)
             .TheNext(20).With(o => o.TaxonId = taxonId2)
             .TheNext(20).With(o => o.TaxonId = taxonId3)
             .TheNext(20).With(o => o.TaxonId = taxonId4)
             .TheNext(20).With(o => o.TaxonId = taxonId5)
            .Build();

        return verbatimObservations;
    }

    public static IList<ArtportalenObservationVerbatim> Create100RecordsWithTaxonCategoryIds(int taxonCategoryId1, int taxonCategoryId2, int taxonCategoryId3, int taxonCategoryId4, int taxonCategoryId5)
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20).HaveTaxonCategoryTaxonId(taxonCategoryId1)
             .TheNext(20).HaveTaxonCategoryTaxonId(taxonCategoryId2)
             .TheNext(20).HaveTaxonCategoryTaxonId(taxonCategoryId3)
             .TheNext(20).HaveTaxonCategoryTaxonId(taxonCategoryId4)
             .TheNext(20).HaveTaxonCategoryTaxonId(taxonCategoryId5)
            .Build();

        return verbatimObservations;
    }
}