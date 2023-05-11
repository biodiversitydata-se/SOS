namespace SOS.UserStatistics.Modules;

public class UserStatisticsModule : Interfaces.IModule
{
    public void MapEndpoints(WebApplication application)
    {
        application.MapGet("userstatistics/clearcache", (IUserStatisticsManager userStatisticsManager) =>
        {
            //userStatisticsManager.ClearCache();
            return Results.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);

        /// <summary>
        /// Aggregates taxon by user.
        /// </summary>
        application.MapPost("userstatistics/pagedspeciescountaggregation", async (IUserStatisticsManager userStatisticsManager,
            [FromBody] SpeciesCountUserStatisticsQuery query, 
            [FromQuery] int? skip, int? take, bool useCache) =>
        {
            var res = await userStatisticsManager.PagedSpeciesCountSearchAsync(query, skip, take, useCache);
            var dto = res.ToPagedResultDto(res.Records);
            return Results.Ok(dto);
        })
        .Produces<PagedResultDto<UserStatisticsItem>>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);

        application.MapPost("userstatistics/pagedspecieslistsummary", async (IUserStatisticsManager userStatisticsManager,
          SpeciesSummaryUserStatisticsQuery query, [FromQuery] int? skip, int? take, bool useCache) =>
        {
            var res = await userStatisticsManager.PagedSpeciesSummaryListAsync(query, skip, take, useCache);
        })
       .Produces<IEnumerable<UserStatisticsByMonthItem>>()
       .Produces(StatusCodes.Status400BadRequest)
       .Produces(StatusCodes.Status401Unauthorized)
       .Produces(StatusCodes.Status500InternalServerError);

        /// <summary>
        /// Aggregates taxon by user.
        /// </summary>
        application.MapPost("userstatistics/speciescountaggregation", async (IUserStatisticsManager userStatisticsManager,
            [FromBody] SpeciesCountUserStatisticsQuery query, 
            [FromQuery] int? skip, 
            [FromQuery] int? take, 
            [FromQuery] bool useCache) =>
        {
            var res = await userStatisticsManager.SpeciesCountSearchAsync(query, skip, take, useCache);
            PagedResultDto<UserStatisticsItem> dto = res.ToPagedResultDto(res.Records);
            return Results.Ok(dto);
        })
        .Produces<PagedResultDto<UserStatisticsItem>>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);

        /// <summary>
        /// Experimental. The functionality should be the same as PagedSpeciesCountAggregation but without the need to create a new index.
        /// Aggregates taxon by user. 
        /// </summary>
        application.MapPost("userstatistics/processedobservationpagedspeciescountaggregation", async (IUserStatisticsManager userStatisticsManager,
            [FromBody] SpeciesCountUserStatisticsQuery query, 
            [FromQuery] int? skip, 
            [FromQuery] int? take, 
            [FromQuery] bool useCache) =>
        {
            var res = await userStatisticsManager.ProcessedObservationPagedSpeciesCountSearchAsync(query, skip, take, useCache);
            var dto = res.ToPagedResultDto(res.Records);
            return Results.Ok(dto);
        })
        .Produces<PagedResult<UserStatisticsItem>>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);

        application.MapGet("userstatistics/speciescountbymonthaggregation", async (IUserStatisticsManager userStatisticsManager,
            [FromQuery] int? taxonId, 
            [FromQuery] int? year, 
            [FromQuery] SpeciesGroup speciesGroup) =>
        {
            await Task.Run(() => throw new NotImplementedException());
        })
        .Produces<IEnumerable<UserStatisticsByMonthItem>>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);

        application.MapGet("userstatistics/speciescountbyyearandmonthaggregation", async (IUserStatisticsManager userStatisticsManager,
            [FromQuery] int? taxonId, 
            [FromQuery] int? year, 
            [FromQuery] SpeciesGroup speciesGroup) =>
        {
            await Task.Run(() => throw new NotImplementedException());
        })
        .Produces<IEnumerable<UserStatisticsByMonthItem>>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}