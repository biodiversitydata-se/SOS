using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Models.Export;
using SOS.Api.Contracts.Exports.Responses;

namespace SOS.Lib.Extensions.Dto;

public static class ExportExtensions
{
    extension(ExportJobInfo exportJobInfo)
    {
        public ExportJobInfoDto ToDto()
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
}
