using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Observations.Api.IntegrationTests.TestData.Factories;

internal static class SearchFilterInternalDtoFactory
{
    public static SearchFilterInternalDto CreateTestFilter()
    {
        return new SearchFilterInternalDto
        {
            Taxon = new TaxonFilterDto
            {
                Ids = [100077] // Utter
            },
            Output = new OutputFilterExtendedDto
            {
                FieldSet = OutputFieldSet.All
            }
        };
    }
}
