namespace SOS.Shared.Api.Dtos.Status;
public class ProcessStatusDto
{
    public string Name { get; set; }
    public string Status { get; set; }
    public int PublicCount { get; set; }
    public int ProtectedCount { get; set; }
    public int InvalidCount { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
