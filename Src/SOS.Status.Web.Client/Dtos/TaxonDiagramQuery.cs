namespace SOS.Status.Web.Client.Dtos;

public class TaxonDiagramQuery
{
    public int[] TaxonIds { get; set; } = Array.Empty<int>();
    public TaxonRelationsTreeIterationMode TreeIterationMode { get; set; } = TaxonRelationsTreeIterationMode.BothParentsAndChildren;
    public bool IncludeSecondaryRelations { get; set; } = true;
    public string TranslationCultureCode { get; set; } = "sv-SE";
}