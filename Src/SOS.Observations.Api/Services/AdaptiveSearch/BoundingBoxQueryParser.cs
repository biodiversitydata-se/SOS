using SOS.Shared.Api.Dtos.Filter;
using System.Globalization;

namespace SOS.Observations.Api.Services.AdaptiveSearch;

internal static class BoundingBoxQueryParser
{
    public static bool TryApply(SearchFilterInternalDto filter, string? bboxQuery, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(bboxQuery))
        {
            return true;
        }

        var parts = bboxQuery.Split(',');
        if (parts.Length != 4)
        {
            error = "The bbox query parameter must contain four comma-separated values: minLongitude,minLatitude,maxLongitude,maxLatitude.";
            return false;
        }

        var coordinates = new double[4];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!double.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out coordinates[i]))
            {
                error = "The bbox query parameter must use invariant decimal numbers: minLongitude,minLatitude,maxLongitude,maxLatitude.";
                return false;
            }
        }

        var minLongitude = coordinates[0];
        var minLatitude = coordinates[1];
        var maxLongitude = coordinates[2];
        var maxLatitude = coordinates[3];
        if (minLongitude >= maxLongitude || minLatitude >= maxLatitude)
        {
            error = "The bbox query parameter must use minLongitude,minLatitude,maxLongitude,maxLatitude where min values are smaller than max values.";
            return false;
        }

        if (minLongitude < -180 || maxLongitude > 180 || minLatitude < -90 || maxLatitude > 90)
        {
            error = "The bbox query parameter must be within WGS84 longitude/latitude bounds.";
            return false;
        }

        filter.Geographics ??= new GeographicsFilterDto();
        filter.Geographics.BoundingBox = new LatLonBoundingBoxDto
        {
            TopLeft = new LatLonCoordinateDto
            {
                Longitude = minLongitude,
                Latitude = maxLatitude
            },
            BottomRight = new LatLonCoordinateDto
            {
                Longitude = maxLongitude,
                Latitude = minLatitude
            }
        };

        return true;
    }
}
