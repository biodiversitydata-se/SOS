using FizzWare.NBuilder;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.ContainerIntegrationTests.TestData;

internal static class ArtportalenTestData
{
    public static IList<ArtportalenObservationVerbatim> Create100RecordsWhere60HasRedlistCategoriesCrEnVu()
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>
            .CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20).HaveRedlistedTaxonId("CR") // Critically Endangered (Akut hotad)
             .TheNext(20).HaveRedlistedTaxonId("EN") // Endangered (Starkt hotad)
             .TheNext(20).HaveRedlistedTaxonId("VU") // Vulnerable (Sårbar)
             .TheNext(20).HaveRedlistedTaxonId("NT") // Near Threatened (Nära hotad)
             .TheNext(20).HaveRedlistedTaxonId(null) // Not redlisted taxa
            .Build();

        return verbatimObservations;
    }

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