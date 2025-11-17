using SOS.Lib.Enums;
using System.Text.Json.Serialization;

namespace SOS.Lib.Models.DataCite;

public class DOIContributor
{
    /// <summary>
    /// Name of creator
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Name type of creator
    /// </summary>
    [JsonPropertyName("nameType")]
    public NameType NameType { get; set; }

    /// <summary>
    /// Given name of creator
    /// </summary>
    [JsonPropertyName("contributorType")]
    public ContributorType ContributorType { get; set; }
}
