using SOS.Api.Contracts.Status.Responses;
using SOS.Lib.Models.Status;

namespace SOS.Lib.Extensions.Dto;

/// <summary>Maps MongoDB jobs to API response DTOs.</summary>
public static class StatusInfoMappingExtensions
{
    public static HangfireJobDto ToHangfireJobDto(this HangfireJob source) => new()
    {
        Id = new ObjectId { Timestamp = source.Id.Timestamp, CreationTime = source.Id.CreationTime },
        CreatedAt = source.CreatedAt,
        StateName = source.StateName,
        InvocationData = source.InvocationData
    };
}
