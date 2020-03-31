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
    /// Artportalen data service
    /// </summary>
    public class ArtportalenDataService : IArtportalenDataService
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor
        /// </summary>
        public ArtportalenDataService(ArtportalenConfiguration artportalenConfiguration)
        {
            _connectionString = artportalenConfiguration?.ConnectionString ?? throw new ArgumentNullException(nameof(artportalenConfiguration));
        }

        /// <summary>
        /// Create new db connection
        /// </summary>
        private IDbConnection Connection => new SqlConnection(_connectionString);

        /// <inheritdoc />
        public async Task<IEnumerable<T>> QueryAsync<T>(string query, dynamic parameters = null)
        {
            using var conn = Connection;
            conn.Open();

            return (await conn.QueryAsync<T>(
                new CommandDefinition(
                    query,
                    parameters,
                    null,
                    5*60, // 5 minutes
                    CommandType.Text,
                    CommandFlags.NoCache
                )
            )).ToArray();
        }
    }
}
