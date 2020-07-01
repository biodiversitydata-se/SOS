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
        private readonly ArtportalenConfiguration _configuration;

        /// <summary>
        ///     Constructor
        /// </summary>
        public ArtportalenDataService(ArtportalenConfiguration artportalenConfiguration)
        {
            _configuration = artportalenConfiguration ??
                             throw new ArgumentNullException(nameof(artportalenConfiguration));
        }

        /// <summary>
        /// Create new db connection
        /// </summary>
        /// <param name="live"></param>
        /// <returns></returns>
        private IDbConnection Connection (bool live )=> new SqlConnection(live ? _configuration.ConnectionStringLive : _configuration.ConnectionStringBackup);

        /// <inheritdoc />
        public async Task<IEnumerable<T>> QueryAsync<T>(string query, dynamic parameters = null, bool live = false)
        {
            using var conn = Connection(live);
            conn.Open();

            return (await conn.QueryAsync<T>(
                new CommandDefinition(
                    query,
                    parameters,
                    null,
                    5 * 60, // 5 minutes
                    CommandType.Text,
                    CommandFlags.NoCache
                )
            )).ToArray();
        }
    }
}