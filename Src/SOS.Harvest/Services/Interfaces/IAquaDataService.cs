using SOS.Lib.Configuration.Import;
using System.Data;

namespace SOS.Harvest.Services.Interfaces;

/// <summary>
///     Aqua data service interface
/// </summary>
public interface IAquaDataService
{
    /// <summary>
    ///  Service configuration
    /// </summary>
    AquaConfiguration Configuration { get; }

    /// <summary>
    /// Query data base
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="parameters"></param>
    /// <param name="commandType"></param>
    /// <returns></returns>
    Task<IEnumerable<T>> QueryAsync<T>(string query, dynamic? parameters = null, CommandType commandType = CommandType.Text);

    /// <summary>
    /// Executes the specified SQL query asynchronously and returns the first column of the first row in the result set
    /// as the specified type.
    /// </summary>
    /// <remarks>If the result set is empty, the default value of type T is returned. Additional columns or
    /// rows are ignored.</remarks>
    /// <typeparam name="T">The type of the value to return from the query result.</typeparam>
    /// <param name="query">The SQL query to execute. This must be a valid statement for the target database.</param>
    /// <param name="parameters">An object containing the parameters to be applied to the SQL query, or null if the query does not require
    /// parameters.</param>
    /// <param name="commandType">Specifies how the command string is interpreted. The default is CommandType.Text.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the value of the first column of the
    /// first row in the result set, cast to the specified type.</returns>
    Task<T> ExecuteScalarAsync<T>(string query, dynamic? parameters = null, CommandType commandType = CommandType.Text);
}