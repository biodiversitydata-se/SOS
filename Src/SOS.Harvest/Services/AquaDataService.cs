using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using System.Data;

namespace SOS.Harvest.Services;

/// <summary>
///     Aqua data service
/// </summary>
public class AquaDataService : IAquaDataService
{
    /// <summary>
    /// Gets a new database connection using the configured connection string.
    /// </summary>
    /// <remarks>Each access returns a new instance of the database connection. The caller is responsible for
    /// opening, closing, and disposing the connection as needed.</remarks>
    private IDbConnection Connection => new SqlConnection(Configuration.ConnectionString);
    protected readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the AquaDataService class with the specified configuration and logger.
    /// </summary>
    /// <param name="aquaConfiguration">The configuration settings used to initialize the service. Cannot be null.</param>
    /// <param name="logger">The logger instance used for logging diagnostic and operational information. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if either aquaConfiguration or logger is null.</exception>
    public AquaDataService(AquaConfiguration aquaConfiguration, ILogger logger)
    {
        Configuration = aquaConfiguration ?? throw new ArgumentNullException(nameof(aquaConfiguration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public AquaConfiguration Configuration { get; private set; }


    /// <inheritdoc />
    public async Task<IEnumerable<T>> QueryAsync<T>(string query, dynamic? parameters = null, CommandType commandType = CommandType.Text)
    {
        try
        {
            using var conn = Connection;
            conn.Open();
            using var transaction = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

            var result = (await conn.QueryAsync<T>(
                new CommandDefinition(
                    query,
                    parameters,
                    transaction,
                    10 * 60, // 10 minutes. Test environemt is slow. Querys taking < 30 sek in prod can take +5 min in test
                    commandType,
                    CommandFlags.NoCache
                )
            )).ToArray();

            transaction.Commit();
            return result;
        }
        catch (Exception e)
        {
            string? connectionString = Configuration?.ConnectionString;
            _logger.LogError(e, "Error when executing QueryAsync(...). ConnectionString=" +connectionString, "Aqua");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<T> ExecuteScalarAsync<T>(string query, dynamic? parameters = null, CommandType commandType = CommandType.Text)
    {
        try
        {
            using var conn = Connection;
            conn.Open();
            using var transaction = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

            var result = await conn.ExecuteScalarAsync<T>(
                new CommandDefinition(
                    query,
                    parameters,
                    transaction,
                    60, // 1 minute. Test environemt is slow.
                    commandType,
                    CommandFlags.NoCache
                )
            );

            transaction.Commit();
            return result ?? default;
        }
        catch (Exception e)
        {
            string? connectionString = Configuration?.ConnectionString;
            _logger.LogError(e, "Error when executing ScalarAsync(...). ConnectionString=" + connectionString, "Aqua");
            throw;
        }
    }
}