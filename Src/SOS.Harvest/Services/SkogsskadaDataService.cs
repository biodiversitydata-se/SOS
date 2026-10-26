using Dapper;
using Npgsql;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using System.Data;

namespace SOS.Harvest.Services;

/// <summary>
///     Skogsskada data service
/// </summary>
public class SkogsskadaDataService : ISkogsskadaDataService
{
    /// <summary>
    /// Create new db connection
    /// </summary>
    /// <param name="live"></param>
    /// <returns></returns>
    private IDbConnection Connection => new NpgsqlConnection(Configuration.ConnectionString);

    /// <summary>
    ///     Constructor
    /// </summary>
    public SkogsskadaDataService(SkogsskadaConfiguration skogsskadaConfiguration)
    {
        Configuration = skogsskadaConfiguration ??
                        throw new ArgumentNullException(nameof(skogsskadaConfiguration));
    }

    /// <inheritdoc />
    public SkogsskadaConfiguration Configuration { get; }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> QueryAsync<T>(string query, dynamic? parameters = null)
    {
        using var conn = Connection;
        conn.Open();

        var transaction = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

        IEnumerable<T> result = null!;
        try
        {
            result = (await conn.QueryAsync<T>(
                new CommandDefinition(
                    query,
                    parameters,
                    transaction,
                    5 * 60, // 5 minutes
                    CommandType.Text,
                    CommandFlags.NoCache
                )
            )).ToArray();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
        }

        return result;
    }
}