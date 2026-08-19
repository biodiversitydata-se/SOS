namespace SOS.Api.Client.Requests;

/// <summary>Authorization headers that can be set explicitly for one SOS request.</summary>
public class SosRoleRequestOptions
{
    /// <summary>The role identifier. An explicit value takes precedence over request context and configured defaults.</summary>
    public int? RoleId { get; set; }
    /// <summary>The calling application identifier. An explicit value takes precedence over request context and configured defaults.</summary>
    public string? AuthorizationApplicationIdentifier { get; set; }
}
