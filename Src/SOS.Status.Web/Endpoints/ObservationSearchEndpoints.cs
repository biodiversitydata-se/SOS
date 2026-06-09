using Microsoft.AspNetCore.Mvc;
using SOS.Status.Web.Client.Dtos.SosObsApi;
using SOS.Status.Web.Client.Models;
using SOS.Status.Web.Services;

namespace SOS.Status.Web.Endpoints;

public static class ObservationSearchEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapObservationSearchEndpoints()
        {
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
                       [FromQuery] int? locationAggregationLimit = null,
                       [FromQuery] AdaptiveSearchLocationsStrategy adaptiveSearchStrategy = AdaptiveSearchLocationsStrategy.ObservationsThenGrid,
                       [FromQuery] bool cluster = true,
                       [FromQuery] string clusteringAlgorithm = "Greedy",
                       [FromQuery] CreatePolygonsMode createPolygons = CreatePolygonsMode.No,
                       [FromQuery] double clusterRadiusPx = 40.0,
                       [FromQuery] int minLocationsPerCluster = 2,
                       [FromQuery] int minLocationCountBeforeClustering = 100,
                       [FromQuery] bool validateSearchFilter = false,
                       [FromQuery] string translationCultureCode = "sv-SE",
                       [FromQuery] bool sensitiveObservations = false,
                       [FromQuery] double? metersPerPixel = null,
                       [FromQuery] string? bbox = null,
                       [FromQuery] bool createSampleObservations = false,
                       [FromQuery] bool useCache = true,
                       [FromQuery] int? cutoffYear = null) =>
                {
                    try
                    {
                        var result = await service.AdaptiveSearchLocationItemsCachedGeoJson(
                            filter,
                            observationsLimit,
                            maxGridCells,
                            locationAggregationLimit,
                            adaptiveSearchStrategy,
                            cluster,
                            clusteringAlgorithm,
                            createPolygons,
                            clusterRadiusPx,
                            minLocationsPerCluster,
                            minLocationCountBeforeClustering,
                            validateSearchFilter,
                            translationCultureCode,
                            sensitiveObservations,
                            metersPerPixel,
                            bbox,
                            createSampleObservations,
                            useCache,
                            cutoffYear);

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

