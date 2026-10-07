namespace SOS.Api.Contracts.Status.Responses;

/// <summary>The timestamp metadata exposed by the existing MongoDB identifier response.</summary>
public class ObjectId
{
    /// <summary>The identifier's timestamp in seconds since the Unix epoch.</summary>
    public int Timestamp { get; set; }

    /// <summary>The identifier's creation time in UTC.</summary>
    public DateTime CreationTime { get; set; }
}
