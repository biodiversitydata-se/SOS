using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Services
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

        /// <summary>
        ///     Constructor
        /// </summary>
        public ArtportalenDataService(ArtportalenConfiguration artportalenConfiguration)
        {
            Configuration = artportalenConfiguration ??
                            throw new ArgumentNullException(nameof(artportalenConfiguration));
        }

        /// <inheritdoc />
        public ArtportalenConfiguration Configuration { get; }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> QueryAsync<T>(string query, dynamic parameters = null, bool live = false)
        {
            using var conn = Connection(live);
            conn.Open();
            var transaction = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

            IEnumerable<T> result = null;
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
            catch (Exception e)
            {
                transaction.Rollback();
            }

            return result;
        }
    }
}