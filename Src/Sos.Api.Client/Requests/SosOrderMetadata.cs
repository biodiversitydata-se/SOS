namespace SOS.Api.Client.Requests;

/// <summary>
/// Metadata shared by composed SOS analysis order workflows.
/// </summary>
public sealed class SosOrderMetadata
{
    /// <summary>The user-facing job description.</summary>
    public required string Description { get; set; }

    /// <summary>
    /// Controls whether completion mail is sent through ZendTo.
    /// </summary>
    public bool SendMailFromZendTo { get; set; } = true;

    /// <summary>
    /// Optional encryption password. The service supplies it to both SOS confirmation parameters.
    /// </summary>
    public string EncryptPassword { get; set; } = string.Empty;
}
