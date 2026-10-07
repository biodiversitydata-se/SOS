using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Taxonomy.Requests;

namespace SOS.Status.Web.Client.Abstractions;

public interface ITaxonDiagramService
{
    Task<string?> GetTaxonRelationsDiagramAsync(
        int[] taxonIds,
        TaxonRelationsTreeIterationMode treeIterationMode = TaxonRelationsTreeIterationMode.BothParentsAndChildren,
        bool includeSecondaryRelations = true,
        TaxonomyType taxonomyType = TaxonomyType.Dyntaxa,
        string translationCultureCode = "sv-SE");
}
