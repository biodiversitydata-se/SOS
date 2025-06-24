namespace SOS.Shared.Api.Dtos.Status;
public class ProcessSummaryDto
{    
    public MongoDbProcessInfoDto ActiveProcessInfo { get; set; }
    public MongoDbProcessInfoDto InactiveProcessInfo { get; set; }
    public List<DataProviderStatusDto> DataProviderStatuses { get; set; }
}