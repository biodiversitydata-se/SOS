namespace SOS.Api.Contracts.Status.Responses;
/// <summary>MongoDB disk-usage information.</summary>
public class MongoDbInfoDto
{
    /// <summary>The reported disk space used by MongoDB.</summary>
    public string? DiskUsed { get; set; }

    /// <summary>The reported total disk space available to MongoDB.</summary>
    public string? DiskTotal { get; set; }
}


