using System;
using System.Collections.Generic;
using System.Linq;
using DqMetricSimulator.Data;

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
            var colId = table.Columns.Select((c,i) => new {c, i}) .Where(c => c.c.Name.Equals(colName,StringComparison.CurrentCultureIgnoreCase))
                .Select(c => (int?)c.i).FirstOrDefault();
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
        public static IRow GetNewTableRow(this ITable newTable, ITable originalTable, IRow originalRow)
        {
            var rv = new Row();
            originalRow.Rows.Select((o, i) => newTable.Columns[i].BinarySearch(GetValue(originalTable, i, originalRow))).ToList()
                .ForEach(idx => rv.Rows.Add(idx));
            return rv;
        }
        public static void FillFromFilter(this ITable newTable, ITable originalTable, IEnumerable<IRow> filter)
        {
            //Fix the columns
            for (var i = 0; i < newTable.Columns.Count; i++)
            {
                newTable.Columns[i] = TableFactory.CreateColumn(originalTable.Columns[i], filter.Select(r => r.Rows[i]));
            }

            filter.ToList().ForEach(r => newTable.Rows.Add(newTable.GetNewTableRow(originalTable, r)) );
            
        }

    }
}
