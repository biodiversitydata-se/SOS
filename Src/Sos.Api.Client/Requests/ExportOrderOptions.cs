namespace SOS.Api.Client.Requests;

/// <summary>Options for queuing an observation export.</summary>
public sealed class ExportOrderOptions : ExportDownloadOptions
{
    /// <summary>The user-facing job description.</summary>
    public required string Description { get; set; }
    /// <summary>
    /// Controls whether completion mail is sent through ZendTo.
    /// </summary>
    public bool SendMailFromZendTo { get; set; } = true;
    /// <summary>The optional export encryption password.</summary>
    public string EncryptPassword { get; set; } = string.Empty;
    /// <summary>The encryption password confirmation.</summary>
    public string ConfirmEncryptPassword { get; set; } = string.Empty;
}
