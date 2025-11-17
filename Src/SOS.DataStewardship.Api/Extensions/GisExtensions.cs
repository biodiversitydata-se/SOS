using SOS.DataStewardship.Api.Contracts.Enums;

namespace SOS.DataStewardship.Api.Extensions;

internal static class GisExtensions
{
    extension(CoordinateSystem coordinateSystem)
    {
        public CoordinateSys ToCoordinateSys()
        {
            return coordinateSystem switch
            {
                CoordinateSystem.EPSG3006 => CoordinateSys.SWEREF99_TM,
                CoordinateSystem.EPSG3857 => CoordinateSys.WebMercator,
                CoordinateSystem.EPSG4258 => CoordinateSys.ETRS89,
                CoordinateSystem.EPSG4619 => CoordinateSys.SWEREF99,
                _ => CoordinateSys.WGS84
            };
        }
    }
}
