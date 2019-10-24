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
    /// Species portal data service
    /// </summary>
    public class SpeciesPortalDataService : ISpeciesPortalDataService
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionStrings"></param>
        public SpeciesPortalDataService(ConnectionStrings connectionStrings)
        {
            _connectionString = connectionStrings?.SpeciesPortal ?? throw new ArgumentNullException(nameof(connectionStrings));
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
                    null,
                    CommandType.Text,
                    CommandFlags.NoCache
                )
            )).ToArray();
        }
    }
}
