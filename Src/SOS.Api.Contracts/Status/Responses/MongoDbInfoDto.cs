namespace SOS.Api.Contracts.Status.Responses;
/// <summary>MongoDB disk-usage information.</summary>
public class MongoDbInfoDto
{
    public string? DiskUsed { get; set; }

    public string? DiskTotal { get; set; }
}


