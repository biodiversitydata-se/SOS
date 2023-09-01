using NetTopologySuite.Features;

namespace SOS.IntegrationTests.Helpers;
internal static class GeoJsonHelper
{
    public static FeatureCollection ReadGeoJsonFile(byte[] file)
    {
        using var memoryStream = new MemoryStream(file);
        var geoJsonString = Encoding.UTF8.GetString(file, 0, file.Length);
        var reader = new NetTopologySuite.IO.GeoJsonReader();
        var featureCollection = reader.Read<FeatureCollection>(geoJsonString);
        return featureCollection;
    }
}
