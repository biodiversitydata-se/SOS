using Microsoft.AspNetCore.Http;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Extensions.Dto;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal static class FilterAreaReferenceFactory
{
    public static IReadOnlyList<FilterAreaReferenceDto> Create(GeographicsFilterDto? filter, PathString pathBase)
    {
        var references = new List<FilterAreaReferenceDto>();
        if (filter?.Areas != null)
        {
            foreach (var area in filter.Areas
                .Where(a => a != null && Enum.IsDefined(a.AreaType) && !string.IsNullOrWhiteSpace(a.FeatureId))
                .DistinctBy(a => (a.AreaType, a.FeatureId)))
            {
                references.Add(new FilterAreaReferenceDto
                {
                    FeatureType = "FilterArea.Area",
                    AreaType = area.AreaType,
                    FeatureId = area.FeatureId,
                    GeometryUrl = $"{pathBase}/areas/{area.AreaType}/{Uri.EscapeDataString(area.FeatureId)}/geometry"
                });
            }
        }

        var geometries = filter.GetSearchGeometries();
        if (geometries == null || geometries.Count == 0) return references;

        // Match the existing inline filter-area representation, including its single point-buffer case.
        var searchCircle = filter!.MaxDistanceFromPoint is > 0 && geometries.FirstOrDefault()?.GeometryType == "Point";
        references.Add(new FilterAreaReferenceDto
        {
            FeatureType = searchCircle ? "FilterArea.Polygon" : "FilterArea.OwnGeometries",
            MaxDistanceFromPoint = searchCircle ? filter.MaxDistanceFromPoint : null
        });

        return references;
    }
}
