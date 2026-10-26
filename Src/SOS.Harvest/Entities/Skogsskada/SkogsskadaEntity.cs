namespace SOS.Harvest.Entities.Skogsskada;

public class SkogsskadaEntity
{
    public string? occurrenceID { get; set; }
    public string? basisOfRecord { get; set; }
    public DateTime? eventDate { get; set; }
    public string? scientificName { get; set; }
    public string? identificationVerificationStatus { get; set; }
    public double? decimalLatitude { get; set; }
    public double? decimalLongitude { get; set; }
    public string? geodeticDatum { get; set; }
    public string? footprintWKT { get; set; }
    public string? footprintSRS { get; set; }
    public string? ownerInstitutionCode { get; set; }
    public string? institutionCode { get; set; }
    public string? occurrenceRemarks { get; set; }
    public string? verbatimCoordinates { get; set; }
    public string? verbatimSRS { get; set; }
    public string? country { get; set; }
    public string? countryCode { get; set; }
    public string? recordedBy { get; set; }
    public string? rightsHolder { get; set; }
    public string? accessRights { get; set; }
    public string? samplingProtocol { get; set; }
}
