using SOS.Status.Web.Client.Dtos;

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
