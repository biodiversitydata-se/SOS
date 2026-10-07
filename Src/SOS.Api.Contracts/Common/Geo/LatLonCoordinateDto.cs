namespace SOS.Api.Contracts.Common.Geo;
/// <summary>A coordinate expressed as latitude and longitude.</summary>
public class LatLonCoordinateDto
{
    /// <summary>Creates a coordinate with latitude and longitude set to zero.</summary>
    public LatLonCoordinateDto() { }

    /// <summary>Creates a coordinate from latitude and longitude in decimal degrees.</summary>
    /// <param name="latitude">The latitude in decimal degrees.</param>
    /// <param name="longitude">The longitude in decimal degrees.</param>
    public LatLonCoordinateDto(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>The latitude in decimal degrees.</summary>
    public double Latitude { get; set; }

    /// <summary>The longitude in decimal degrees.</summary>
    public double Longitude { get; set; }

}


