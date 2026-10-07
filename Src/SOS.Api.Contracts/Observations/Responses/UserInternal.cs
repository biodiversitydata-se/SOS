namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>User information and access flags associated with an observation.</summary>
public class UserInternal
{

    /// <summary>
    /// User discovered the observation
    /// </summary>
    public bool Discover { get; set; }

    /// <summary>
    /// User Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Person Id
    /// </summary>
    public int PersonId { get; set; }

    /// <summary>
    /// User Service id
    /// </summary>
    public int? UserServiceUserId { get; set; }

    /// <summary>
    /// User alias
    /// </summary>
    public string? UserAlias { get; set; }

    /// <summary>
    /// User with sort &gt; 0 is authorized to view the observation
    /// </summary>
    public bool? ViewAccess { get; set; }

}


