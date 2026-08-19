namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// Taxon vernacular name.
/// </summary>
public class TaxonVernacularName
{

    /// <summary>
    /// A common vernacular name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// ISO 639-1 language code used for the vernacular name value..
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// The standard code for the country in which the vernacular name is used.
    /// <br/>Recommended best practice is to use the ISO 3166-1-alpha-2 country codes available as a vocabulary
    /// <br/>at http://rs.gbif.org/vocabulary/iso/3166-1_alpha2.xml. For multiple countries separate values with a comma ","
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// This term is true if the source citing the use of this vernacular name indicates the usage has 
    /// <br/>some preference or specific standing over other possible vernacular names used for the species.
    /// </summary>
    public bool IsPreferredName { get; set; }

    /// <summary>
    /// Valid for sighting
    /// </summary>
    public bool ValidForSighting { get; set; }

}


