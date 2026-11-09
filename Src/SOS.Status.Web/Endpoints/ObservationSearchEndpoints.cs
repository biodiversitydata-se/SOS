using SOS.Status.Web.Client.Models;
﻿﻿using Microsoft.AspNetCore.Mvc;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Status.Web.Services;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Exceptions;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Areas;

namespace SOS.Status.Web.Endpoints;

public static class ObservationSearchEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapObservationSearchEndpoints()
        {
            app.MapGet("/api/areas/{areaType}/{featureId}/geometry",
                async ([FromServices] ISosAreasClient areas, HttpContext context,
                    AreaTypeDto areaType, string featureId) =>
                {
                    if (!Enum.IsDefined(areaType) || string.IsNullOrWhiteSpace(featureId))
                        return Results.BadRequest("A valid areaType and featureId are required.");
                    try
                    {
                        var result = await areas.GetAreaGeometryAsync(areaType, featureId,
                            context.Request.Headers.IfNoneMatch.ToString(), context.RequestAborted);
                        context.Response.Headers.CacheControl = AreaGeometryCacheDefaults.CacheControl;
                        if (result.ETag != null) context.Response.Headers.ETag = result.ETag;
                        return result.NotModified
                            ? Results.StatusCode(StatusCodes.Status304NotModified)
                            : Results.Text(result.GeoJson, "application/geo+json; charset=utf-8");
                    }
                    catch (SosApiException exception)
                    {
                        return Results.StatusCode((int)exception.StatusCode);
                    }
                    catch (FormatException)
                    {
                        return Results.BadRequest("Invalid If-None-Match header.");
                    }
                });

            app.MapPost("/api/observations/search",
                async ([FromServices] ObservationSearchService service,
                       [FromBody] SearchFilterInternalDto filter,
                       [FromQuery] int skip = 0,
                       [FromQuery] int take = 100,
                       [FromQuery] string sortBy = "",
                       [FromQuery] string sortOrder = "Asc",
                       [FromQuery] bool validateSearchFilter = false,
                       [FromQuery] string translationCultureCode = "sv-SE",
                       [FromQuery] bool sensitiveObservations = false) =>
                {
                    try
                    {
                        var result = await service.SearchObservations(
                            filter,
                            skip,
                            take,
                            sortBy,
                            sortOrder,
                            validateSearchFilter,
                            translationCultureCode,
                            sensitiveObservations);
                        
                        return Results.Ok(result);
                    }
                    catch (SosApiException ex)
                    {
                        return Results.Problem(statusCode: (int)ex.StatusCode, detail: ex.ResponseBody ?? ex.Message);
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(ex.Message);
                    }
                });

            app.MapPost("/api/observations/searchbycursor",
                async ([FromServices] ObservationSearchService service,
                       [FromBody] SearchFilterInternalDto filter,
                       [FromQuery] int take = 1000,
                       [FromQuery] string? cursor = null,
                       [FromQuery] string sortBy = "taxon.id",
                       [FromQuery] string sortOrder = "Asc",
                       [FromQuery] bool validateSearchFilter = false,
                       [FromQuery] string translationCultureCode = "sv-SE",
                       [FromQuery] bool sensitiveObservations = false) =>
                {
                    var result = await service.SearchObservationsByCursor(
                        filter,
                        take,
                        cursor,
                        sortBy,
                        sortOrder,
                        validateSearchFilter,
                        translationCultureCode,
                        sensitiveObservations);

                    if (result.IsFailure)
                    {
                        return Results.BadRequest(new { error = result.Error });
                    }

                    return Results.Ok(result.Value);
                });                  

            app.MapPost("/api/observations/adaptivesearchlocations",
                async ([FromServices] ObservationSearchService service,
                       HttpContext httpContext,
                       [FromBody] SearchFilterInternalDto filter,
                       [FromQuery] int observationsLimit = 10000,
                       [FromQuery] int maxGridCells = 10000,
                       [FromQuery] int locationAggregationLimit = 1000,
                       [FromQuery] AdaptiveSearchLocationsStrategy adaptiveSearchStrategy = AdaptiveSearchLocationsStrategy.Auto,
                       [FromQuery] bool cluster = true,
                       [FromQuery] double? clusterRadiusMeters = null,
                       [FromQuery] CreatePolygonsMode createPolygons = CreatePolygonsMode.No,
                       [FromQuery] double clusterRadiusPx = AdaptiveSearchLocationsDefaults.ClusterRadiusPx,
                       [FromQuery] int minLocationsPerCluster = 2,
                       [FromQuery] int minLocationCountBeforeClustering = AdaptiveSearchLocationsDefaults.MinLocationCountBeforeClustering,
                       [FromQuery] bool validateSearchFilter = false,
                       [FromQuery] string translationCultureCode = "sv-SE",
                       [FromQuery] bool sensitiveObservations = false,
                       [FromQuery] double? metersPerPixel = null,
                       [FromQuery] string? bbox = null,
                       [FromQuery] bool createSampleObservations = false,
                       [FromQuery] bool useCache = true,
                       [FromQuery] int? cutoffYear = null,
                       [FromQuery] bool includeFilterAreasInResult = false,
                       [FromQuery] double switchToDetailClusteringAtMetersPerPixel = AdaptiveSearchLocationsDefaults.SwitchToDetailClusteringAtMetersPerPixel,
                       [FromQuery] double detailClusterRadiusPx = AdaptiveSearchLocationsDefaults.DetailClusterRadiusPx) =>
                {
                    try
                    {
                        var result = await service.AdaptiveSearchLocationItemsCachedGeoJson(filter,
                            new LocationMapSearchOptions
                            {
                                ObservationsLimit = observationsLimit,
                                MaxGridCells = maxGridCells,
                                LocationAggregationLimit = locationAggregationLimit,
                                AdaptiveSearchStrategy = adaptiveSearchStrategy,
                                Cluster = cluster,
                                ClusterRadiusMeters = clusterRadiusMeters,
                                CreatePolygons = createPolygons,
                                ClusterRadiusPx = clusterRadiusPx,
                                MinLocationsPerCluster = minLocationsPerCluster,
                                MinLocationCountBeforeClustering = minLocationCountBeforeClustering,
                                ValidateSearchFilter = validateSearchFilter,
                                TranslationCultureCode = translationCultureCode,
                                SensitiveObservations = sensitiveObservations,
                                MetersPerPixel = metersPerPixel,
                                Bbox = bbox,
                                CreateSampleObservations = createSampleObservations,
                                UseCache = useCache,
                                CutoffYear = cutoffYear,
                                IncludeFilterAreasInResult = includeFilterAreasInResult,
                                SwitchToDetailClusteringAtMetersPerPixel = switchToDetailClusteringAtMetersPerPixel,
                                DetailClusterRadiusPx = detailClusterRadiusPx,
                            }, httpContext.RequestAborted);

                        if (result.ObservationsTotalCount.HasValue)
                        {
                            httpContext.Response.Headers["X-Observations-TotalCount"] = result.ObservationsTotalCount.Value.ToString();
                        }

                        if (result.EstimatedLocationCount.HasValue)
                        {
                            httpContext.Response.Headers["X-Estimated-Location-Count"] = result.EstimatedLocationCount.Value.ToString();
                        }

                        if (result.FeatureCount.HasValue)
                        {
                            httpContext.Response.Headers["X-Feature-Count"] = result.FeatureCount.Value.ToString();
                        }

                        if (result.LocationsBeforeClustering.HasValue)
                        {
                            httpContext.Response.Headers["X-Locations-Before-Clustering"] = result.LocationsBeforeClustering.Value.ToString();
                        }

                        if (!string.IsNullOrWhiteSpace(result.CacheStatus))
                        {
                            httpContext.Response.Headers["X-Cache"] = result.CacheStatus;
                        }

                        if (!string.IsNullOrWhiteSpace(result.ResultType))
                        {
                            httpContext.Response.Headers["X-Result-Type"] = result.ResultType;
                        }

                        if (!string.IsNullOrWhiteSpace(result.Timings))
                        {
                            httpContext.Response.Headers["X-AdaptiveSearch-Timings"] = result.Timings;
                        }

                        if (result.LocationSummaryApproximated.HasValue)
                            httpContext.Response.Headers["X-Location-Summary-Approximated"] = result.LocationSummaryApproximated.Value ? "true" : "false";
                        if (result.LocationPresentationStatusApproximated.HasValue)
                            httpContext.Response.Headers["X-Location-Presentation-Status-Approximated"] = result.LocationPresentationStatusApproximated.Value ? "true" : "false";
                        if (result.AdaptiveSearchStrategy.HasValue)
                            httpContext.Response.Headers["X-AdaptiveSearch-Strategy"] = result.AdaptiveSearchStrategy.Value.ToString();

                        return Results.Text(result.GeoJson, "application/geo+json");
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(ex.Message);
                    }
                });

            return app;
        }
    }
}
