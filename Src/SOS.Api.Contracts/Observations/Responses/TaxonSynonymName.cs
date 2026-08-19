namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// Taxon synonym.
/// </summary>
public class TaxonSynonymName
{

    /// <summary>
    /// Name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Author.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// The status of the use of the scientificName as a label
    /// <br/>for a taxon. Requires taxonomic opinion to define the
    /// <br/>scope of a taxon. Rules of priority then are used to
    /// <br/>define the taxonomic status of the nomenclature contained
    /// <br/>in that scope, combined with the experts opinion.
    /// <br/>It must be linked to a specific taxonomic reference that
    /// <br/>defines the concept.
    /// <br/>Recommended best practice is to use a controlled vocabulary.
    /// </summary>
    public string? TaxonomicStatus { get; set; }

    /// <summary>
    /// The status related to the original publication of the
    /// <br/>name and its conformance to the relevant rules of
    /// <br/>nomenclature. It is based essentially on an algorithm
    /// <br/>according to the business rules of the code.
    /// <br/>It requires no taxonomic opinion.
    /// </summary>
    public string? NomenclaturalStatus { get; set; }

}


