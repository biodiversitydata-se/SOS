namespace SOS.Shared.Api.Dtos.Status;
public class DataProviderStatusDto
{
    public int Id { get; set; }
    public string Identifier { get; set; }
    public string Name { get; set; }
    public string SwedishName { get; set; }
    public int PublicActive { get; set; }
    public int PublicInactive { get; set; }
    public int PublicDiff { get; set; }
    public int ProtectedActive { get; set; }
    public int ProtectedInactive { get; set; }
    public int ProtectedDiff { get; set; }
    public int InvalidActive { get; set; }
    public int InvalidInactive { get; set; }
    public int InvalidDiff { get; set; }
}
