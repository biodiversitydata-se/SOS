namespace SOS.UserStatistics.Modules;

public class UserStatisticsModule : IModule
{
    public void MapEndpoints(WebApplication application)
    {
        application.MapGet("userstatistics/clearcache", (IUserStatisticsManager userStatisticsManager) =>
        {
            userStatisticsManager.ClearCache();
            return Results.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);

        application.MapPost("userstatistics/pagedspeciescountaggregation", async (IUserStatisticsManager userStatisticsManager,
            SpeciesCountUserStatisticsQuery query, int? skip, int? take, bool useCache) =>
        {
            var res = await userStatisticsManager.PagedSpeciesCountSearchAsync(query, skip, take, useCache);
            var dto = res.ToPagedResultDto(res.Records);
            return Results.Ok(dto);
        })
        .Produces<PagedResultDto<UserStatisticsItem>>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);

        application.MapPost("userstatistics/speciescountaggregation", async (IUserStatisticsManager userStatisticsManager,
            SpeciesCountUserStatisticsQuery query, int? skip, int? take, bool useCache) =>
        {
            var res = await userStatisticsManager.SpeciesCountSearchAsync(query, skip, take, useCache);
            PagedResultDto<UserStatisticsItem> dto = res.ToPagedResultDto(res.Records);
            return Results.Ok(dto);
        })
        .Produces<PagedResultDto<UserStatisticsItem>>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);

        application.MapPost("userstatistics/processedobservationpagedspeciescountaggregation", async (IUserStatisticsManager userStatisticsManager,
            SpeciesCountUserStatisticsQuery query, int? skip, int? take, bool useCache) =>
        {
            var res = await userStatisticsManager.ProcessedObservationPagedSpeciesCountSearchAsync(query, skip, take, useCache);
            var dto = res.ToPagedResultDto(res.Records);
        })
        .Produces<PagedResult<UserStatisticsItem>>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);

        application.MapGet("userstatistics/speciescountbymonthaggregation", async (IUserStatisticsManager userStatisticsManager,
            int? taxonId, int? year, SpeciesGroup speciesGroup) =>
        {
            await Task.Run(() => throw new NotImplementedException());
        })
        .Produces<IEnumerable<UserStatisticsByMonthItem>>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);

        application.MapGet("userstatistics/speciescountbyyearandmonthaggregation", async (IUserStatisticsManager userStatisticsManager,
            int? taxonId, int? year, SpeciesGroup speciesGroup) =>
        {
            await Task.Run(() => throw new NotImplementedException());
        })
        .Produces<IEnumerable<UserStatisticsByMonthItem>>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);

        application.MapGet("userstatistics/specieslistsummary", async (IUserStatisticsManager userStatisticsManager,
            int? taxonId, int? year, SpeciesGroup? speciesGroup, AreaType? areaType, string featureId, int? siteId, string sortBy) =>
        {
            await Task.Run(() => throw new NotImplementedException());
        })
        .Produces<IEnumerable<UserStatisticsByMonthItem>>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}
