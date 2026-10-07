namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Identifies the currently active SOS processing instance.</summary>
public class ActiveInstanceInfoDto
{
    /// <summary>Identifier of the active processing instance.</summary>
    public byte ActiveInstance { get; set; }

    /// <summary>Identifier of the active instance record.</summary>
    public string? Id { get; set; }
}


