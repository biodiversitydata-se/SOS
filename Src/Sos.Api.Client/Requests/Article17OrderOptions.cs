namespace SOS.Api.Client.Requests;

/// <summary>Options for queuing an Article 17 analysis job.</summary>
public sealed class Article17OrderOptions : Article17Options
{
    /// <summary>The user-facing job description.</summary>
    public required string Description { get; set; }
    /// <summary>Controls whether completion mail is sent through ZendTo.</summary>
    public bool SendMailFromZendTo { get; set; } = true;
    /// <summary>The optional export encryption password.</summary>
    public string EncryptPassword { get; set; } = string.Empty;
    /// <summary>The encryption password confirmation.</summary>
    public string ConfirmEncryptPassword { get; set; } = string.Empty;
}
