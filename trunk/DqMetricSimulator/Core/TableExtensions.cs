using System;
using System.Linq;

namespace DqMetricSimulator.Core
{
    public static class TableExtensions
    {
        public static object GetValue(this ITable table, int idx, IRow row)
        {
            return table.Columns[idx][row.Rows[idx]];
        }

        public static object GetValueByColumn(this ITable table, string colName, IRow row)
        {
            var colId = table.Columns.Where(c => c.Name == colName).Select((c, i) => (int?)i).FirstOrDefault();
            if (colId == null)
                throw new ArgumentOutOfRangeException(String.Format("Column '{0}' not found.", colName));
            return GetValue(table, colId.Value, row);
        }
        public static string GetKeystring(this ITable table, IRow row)
        {
            var keystring = "";
            table.Metadata.GetKeyValuesForRow(table, row).ToList().ForEach( k => keystring+=String.Format("({0})", k.ToString()));
            return keystring;
        }
    }
}
