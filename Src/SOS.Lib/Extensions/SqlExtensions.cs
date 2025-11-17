using Microsoft.Data.SqlClient.Server;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SOS.Lib.Extensions;

public static class SqlExtensions
{
    extension(IEnumerable<int> list)
    {
        /// <summary>
        /// Cast list of integers to list of SqlDataRecords
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SqlDataRecord> ToSqlRecords()
        {
            if (!list?.Any() ?? true)
            {
                yield break;
            }
            var record = new SqlDataRecord(new SqlMetaData("Value", SqlDbType.Int));

            foreach (var item in list)
            {
                record.SetInt32(0, item);
                yield return record;
            }
        }
    }
}
