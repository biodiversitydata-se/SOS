namespace SOS.Api.Contracts.Status.Responses;

/// <summary>Status information about a data-provider harvest.</summary>
public class HarvestInfoDto
{
    /// <summary>The harvest identifier.</summary>
    public string Id { get; set; } = null!;

    /// <summary>The number of harvested records.</summary>
    public int Count { get; set; }

    /// <summary>When the source data was last modified.</summary>
    public DateTime? DataLastModified { get; set; }

    /// <summary>When the harvest ended.</summary>
    public DateTime End { get; set; }

    /// <summary>When the harvest started.</summary>
    public DateTime Start { get; set; }

    /// <summary>The harvest status.</summary>
    public string Status { get; set; } = null!;

    /// <summary>Additional information about the harvest.</summary>
    public string Notes { get; set; } = null!;
}
