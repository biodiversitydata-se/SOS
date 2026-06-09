using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Aqua;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Extensions;
using System.Data;

namespace SOS.Harvest.Repositories.Source.Aqua;

/// <summary>
///     Database base class
/// </summary>
public abstract class AquaBaseRepository
{
    /// <summary>
    /// Asynchronously counts the number of records in the specified database view.
    /// </summary>
    /// <param name="viewName">The name of the database view to count records from. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of records in the
    /// specified view.</returns>
    protected async Task<int> CountAsync(string viewName)
    {
        var query = $"select count(*) from {viewName};";
        return await DataService.ExecuteScalarAsync<int>(query);
    }

    /// <summary>
    /// Data service
    /// </summary>
    protected IAquaDataService DataService
    {
        get;
    }

    protected async Task<IEnumerable<SbdiEmof>?> GetEmofsChunkAsync(string emofViewName, int offset, int take)
    {
        try
        {
            var query = @$"
                select *
                from 
                    {emofViewName} e   
                order by 
                    e.eventID 
                offset @offset rows
                fetch next @take rows only;";

            var result = (await QueryAsync<SbdiEmof>(query, new { offset, take }))?.ToArray();
            if ((result?.Count() ?? 0) == 0)
            {
                Logger.LogDebug($"Aqua EmofRepository.GetChunkAsync returned no emofs. view={emofViewName}");
            }

            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error getting emofs. view={emofViewName}");

            throw;
        }
    }

    protected async Task<IEnumerable<SbdiEvent>?> GetEventsChunkAsync(string eventViewName, int offset, int take)
    {
        try
        {
            var query = @$"
                select *
                from 
                    {eventViewName} e
                order by 
                    e.eventID 
                offset @offset rows
                fetch next @take rows only;";

            var result = (await QueryAsync<SbdiEvent>(query, new { offset, take }))?.ToArray();
            if ((result?.Count() ?? 0) == 0)
            {
                Logger.LogDebug($"Aqua EventRepository.GetChunkAsync returned no events. view={eventViewName}, offset={offset}, take={take}");
            }

            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error getting events");

            throw;
        }
    }

    protected async Task<IEnumerable<SbdiOccurrence>?> GetOccurrencesChunkAsync(string occurrenceViewName, int offset, int take)
    {
        try
        {
            var query = @$"
                select *
                from 
                    {occurrenceViewName} e
                order by 
                    e.eventID 
                offset @offset rows
                fetch next @take rows only;";

            var result = (await QueryAsync<SbdiOccurrence>(query, new { offset, take }))?.ToArray();
            if ((result?.Count() ?? 0) == 0)
            {
                Logger.LogDebug($"Aqua OccurrenceRepository.GetChunkAsync returned no occurrences. view={occurrenceViewName}, offset={offset}, take={take}");
            }

            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error getting occurrences");

            throw;
        }
    }

    /// <summary>
    /// Executes the specified query asynchronously and returns the result as a collection of entities of type E.
    /// </summary>
    /// <typeparam name="E">The type of the entities to return. Must be compatible with the result set of the query.</typeparam>
    /// <param name="query">The SQL query or command to execute against the data source.</param>
    /// <param name="parameters">An object containing the parameters to be passed to the query, or null if the query does not require parameters.</param>
    /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of entities
    /// of type E returned by the query.</returns>
    protected async Task<IEnumerable<E>> QueryAsync<E>(string query, dynamic? parameters = null!, CommandType commandType = CommandType.Text)
    {
        return await DataService.QueryAsync<E>(query, parameters, commandType);
    }

    /// <summary>
    ///     Logger
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="dataService"></param>
    /// <param name="logger"></param>
    public AquaBaseRepository(IAquaDataService dataService, ILogger logger)
    {
        DataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}