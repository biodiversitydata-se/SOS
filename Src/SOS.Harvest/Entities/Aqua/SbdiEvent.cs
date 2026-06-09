
namespace SOS.Harvest.Entities.Aqua;

public class SbdiEvent
{
    public string EventID { get; set; }
    public string EventDate { get; set; }
    public string SamplingProtocol { get; set; }
    public string SamplingEffort { get; set; }
    public string AccessRights { get; set; }
    public string Locality { get; set; }
    public string LocationID { get; set; }
    public string DecimalLatitude { get; set; }
    public string DecimalLongitude { get; set; }
    public string GeodeticDatum { get; set; }
    public string LocationRemarks { get; set; }
    public string MinimumDepthInMeters { get; set; }
    public string MaximumDepthInMeters { get; set; }
    public string InformationWithheld { get; set; }
    public string EventRemarks { get; set; }
}
