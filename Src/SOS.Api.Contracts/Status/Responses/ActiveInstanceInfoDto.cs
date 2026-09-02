namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Identifies the currently active SOS processing instance.</summary>
public class ActiveInstanceInfoDto
{
    public byte ActiveInstance { get; set; }

    public string? Id { get; set; }
}


