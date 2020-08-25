using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SOS.Lib.Extensions
{
    public static class SqlExtensions
    {
        public static DataTable ToDataTable(this IEnumerable<int> ids)
        {
            var tvpTable = new DataTable();
            tvpTable.Columns.Add(new DataColumn("Id", typeof(int)));

            if (ids?.Any() ?? false)
            {
                foreach (var id in ids)
                {
                    tvpTable.Rows.Add(id);
                }
            }

            return tvpTable;
        }
    }
}
