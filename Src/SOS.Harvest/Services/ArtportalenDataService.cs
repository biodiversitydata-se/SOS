﻿using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Harvest.Services
{
    /// <summary>
    ///     Artportalen data service
    /// </summary>
    public class ArtportalenDataService : IArtportalenDataService
    {
        /// <summary>
        /// Create new db connection
        /// </summary>
        /// <param name="live"></param>
        /// <returns></returns>
        private IDbConnection Connection(bool live) => new SqlConnection(live ? Configuration.ConnectionStringLive : Configuration.ConnectionStringBackup);
        private readonly ILogger<ArtportalenDataService> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        public ArtportalenDataService(ArtportalenConfiguration artportalenConfiguration, ILogger<ArtportalenDataService> logger)
        {
            Configuration = artportalenConfiguration ??
                            throw new ArgumentNullException(nameof(artportalenConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public string BackUpDatabaseName
        {
            get
            {
                var builder = new SqlConnectionStringBuilder(Configuration.ConnectionStringBackup);

                return builder.InitialCatalog;
            }
        }

        /// <inheritdoc />
        public ArtportalenConfiguration Configuration { get; }

       
        /// <inheritdoc />
        public async Task<IEnumerable<T>> QueryAsync<T>(string query, dynamic parameters = null, bool live = false)
        {
            using var conn = Connection(live);
            conn.Open();
            var transaction = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

           
            try
            {
                var result = (await conn.QueryAsync<T>(
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
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when executing QueryAsync(...)");
                transaction.Rollback();
                throw;
            }
        }
    }
}