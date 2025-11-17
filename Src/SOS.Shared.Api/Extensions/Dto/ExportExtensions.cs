using SOS.Lib.Models.Export;
using SOS.Shared.Api.Dtos.Export;

namespace SOS.Shared.Api.Extensions.Dto;

public static class ExportExtensions
{
    public static ExportJobInfoDto ToDto(this ExportJobInfo exportJobInfo)
    {
        if (exportJobInfo == null)
        {
            return null!;
        }
        return new ExportJobInfoDto
        {
            CreatedDate = exportJobInfo.CreatedDate,
            Description = exportJobInfo.Description,
            ExpireDate = exportJobInfo.ProcessEndDate.HasValue ? exportJobInfo.ProcessEndDate.Value.AddDays(exportJobInfo.LifetimeDays) : null,
            Format = exportJobInfo.Format,
            Id = exportJobInfo.Id,
            NumberOfObservations = exportJobInfo.NumberOfObservations,
            OutputFieldSet = exportJobInfo.OutputFieldSet,
            PickUpUrl = exportJobInfo.PickUpUrl,
            ProcessEndDate = exportJobInfo.ProcessEndDate,
            ProcessStartDate = exportJobInfo.ProcessStartDate,
            Status = exportJobInfo.Status
        };
    }
}