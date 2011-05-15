using System.Collections.Generic;
using System.Data;
using System.Linq;
using DqMetricSimulator.Core;

namespace DqMetricSimulator.Data
{
    /// <summary>
    /// This class provides tools to import data from Ado.net DataTable
    /// </summary>
    public static class ImportDataTable
    {
        /// <summary>
        /// Import data table. Performance is not a big consideration.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static ITable ImportAdoNetDataTable(DataTable dataTable)
        {
            //1.Create the ITable based on the dataTable schema
            //2.Load each column in an IColumn
            //3.Rebuild the table, lookup every column value
            
            var table = ExtractSchema(dataTable);
            (from DataRow row in dataTable.Rows select row).Select( r => GetTableRow(table, r) ).ToList()
                .ForEach(r => table.Rows.Add(r));
            return table;
        }

        private static IRow GetTableRow(ITable table, DataRow r)
        {
            var rv = new Row();
            r.ItemArray.Select((o, i) => table.Columns[i].BinarySearch(o)).ToList()
                .ForEach(idx => rv.Rows.Add(idx));
            return rv;
        }

        private static IColumn CreateAndFillColumn(DataColumn dataColumn)
        {
            var colId = dataColumn.Table.Columns.IndexOf(dataColumn);
            var colData = (from DataRow row in dataColumn.Table.Rows select row[colId]).ToList();
            return TableFactory.CreateColumn(dataColumn, colData.OrderBy(o => o).Distinct());
        }

        private static ITable ExtractSchema(DataTable dataTable)
        {
            //Get keys
            var keys = dataTable.PrimaryKey.Select(k => dataTable.Columns.IndexOf(k.ColumnName)).ToList();
            var table = TableFactory.CreateTable(dataTable.Columns.Count, keys);
            table.Columns.Select(
                (c,i) => 
                    new { i,
                        col = CreateAndFillColumn(dataTable.Columns[i])
                    }
                ).ToList()
                .ForEach(c => table.Columns[c.i] = c.col );
            return table;
        }
        
    }
}
