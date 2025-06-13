namespace SOS.Status.Web.Models;

public class DataProviderStatusRow
{
    public string Datasource { get; set; } = string.Empty;
    public int DataProviderId { get; set; }
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
