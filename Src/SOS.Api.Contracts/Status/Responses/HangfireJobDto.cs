namespace SOS.Api.Contracts.Status.Responses;

/// <summary>Status information about a processing job.</summary>
public class HangfireJobDto
{
    /// <summary>The timestamp metadata of the job's database identifier.</summary>
    public ObjectId Id { get; set; } = null!;

    /// <summary>When the job was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>The current job state.</summary>
    public string StateName { get; set; } = null!;

    /// <summary>The serialized job invocation.</summary>
    public string InvocationData { get; set; } = null!;
}
