using SOS.Api.Contracts.Observations.Requests;
using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Exports.Responses;

/// <summary>
/// Describes a queued or completed SOS export job.
/// </summary>
public sealed class ExportJobInfoDto
{
    /// <summary>The export job identifier.</summary>
    public string? Id { get; set; }
    /// <summary>When the job was created.</summary>
    public DateTime CreatedDate { get; set; }
    /// <summary>When the generated file expires.</summary>
    public DateTime? ExpireDate { get; set; }
    /// <summary>When processing started.</summary>
    public DateTime? ProcessStartDate { get; set; }
    /// <summary>When processing ended.</summary>
    public DateTime? ProcessEndDate { get; set; }
    /// <summary>The total processing time.</summary>
    public TimeSpan? ProcessingTime => ProcessEndDate.HasValue && ProcessStartDate.HasValue
        ? ProcessEndDate.Value - ProcessStartDate.Value
        : null;
    /// <summary>
    /// The number of items represented by the job. Observation exports use the observation count;
    /// AOO/EOO jobs use the taxon count.
    /// </summary>
    public int? NumberOfObservations { get; set; }
    /// <summary>The user-provided job description.</summary>
    public string? Description { get; set; }

    /// <summary>The export format.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ExportFormat Format { get; set; }

    /// <summary>The processing status.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ExportJobStatus Status { get; set; }

    /// <summary>The selected observation output field set.</summary>
    public OutputFieldSet? OutputFieldSet { get; set; }

    /// <summary>The URL from which a completed export can be downloaded.</summary>
    public string? PickUpUrl { get; set; }
}
