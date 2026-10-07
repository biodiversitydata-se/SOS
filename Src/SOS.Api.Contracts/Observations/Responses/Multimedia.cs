namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// Simple Multimedia extension
/// http://rs.gbif.org/extension/gbif/1.0/multimedia.xml
/// </summary>
public class Multimedia
{

    /// <summary>Comments associated with the media resource.</summary>
    public ICollection<MultimediaComment>? Comments { get; set; }

    /// <summary>The type of media resource, such as an image, sound or video.</summary>
    public string? Type { get; set; }

    /// <summary>The file format or MIME type of the media resource.</summary>
    public string? Format { get; set; }

    /// <summary>The identifier or URL of the media resource.</summary>
    public string? Identifier { get; set; }

    /// <summary>A reference or URL providing information about the media resource.</summary>
    public string? References { get; set; }

    /// <summary>The title of the media resource.</summary>
    public string? Title { get; set; }

    /// <summary>A description of the media resource.</summary>
    public string? Description { get; set; }

    /// <summary>The creation date of the media resource, as supplied by the provider.</summary>
    public string? Created { get; set; }

    /// <summary>The person or organisation that created the media resource.</summary>
    public string? Creator { get; set; }

    /// <summary>A person or organisation that contributed to the media resource.</summary>
    public string? Contributor { get; set; }

    /// <summary>The person or organisation that published the media resource.</summary>
    public string? Publisher { get; set; }

    /// <summary>The intended audience of the media resource.</summary>
    public string? Audience { get; set; }

    /// <summary>The source from which the media resource was derived.</summary>
    public string? Source { get; set; }

    /// <summary>The license governing use of the media resource.</summary>
    public string? License { get; set; }

    /// <summary>The person or organisation holding rights to the media resource.</summary>
    public string? RightsHolder { get; set; }

    /// <summary>The identifier of the dataset associated with the media resource.</summary>
    public string? DatasetID { get; set; }

}


