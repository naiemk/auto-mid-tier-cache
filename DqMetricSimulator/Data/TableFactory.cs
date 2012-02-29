using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Common;
using DqMetricSimulator.Core;
using DqMetricSimulator.Query;

namespace DqMetricSimulator.Data
{
    public static class TableFactory
    {
        public static ITable CreateTable(int columns, IEnumerable<int> keys)
        {
            var meta = new Metadata();
            var cols = new List<IColumn>(columns);
            for(var i= 0; i<columns; i++)cols.Add(new Column<string>());
            keys.ToList().ForEach(k => meta.KeyCols.Add(k));
            return new Table
                (
                    meta, new List<IRow>(), cols
                );
        }

        public static ITable CreateTable(IQuery query)
        {
            var tbl = CreateTable(query.Projections.Count, query.GetKeyColumnsIds());
            query.Projections.Select((p, i)=>tbl.Columns[i].Name=p.Columns.First()).ToList();
            return tbl;
        }
        public static ITable CreateTable(ITable source)
        {
            var meta = new Metadata();
            var cols = new List<IColumn>(source.Columns.Count);
            cols.AddRange(source.Columns.Select(t => CreateColumn(t.GetType().GetGenericArguments()[0], null)));
            source.Metadata.KeyCols.ToList().ForEach(k => meta.KeyCols.Add(k));
            return new Table
                (
                    meta, new List<IRow>(), cols
                );
        }

        public static IColumn CreateColumn(Type dataType, IEnumerable<object> data)
        {
            if (data == null) data = new List<object>();
            if (new[]{typeof(Int64), typeof(Int32), typeof(Int16), typeof(UInt16), typeof(UInt32), typeof(UInt64), typeof(int)}.Contains(dataType))
                return new Column<Int64>( data.Where(d => d!= null).Select(d => (Int64)Convert.ChangeType(d, typeof(Int64))) );
            if (dataType == typeof (DateTime))
                return new Column<DateTime>(data.Cast<DateTime>());
            if (dataType == typeof (Double))
                return new Column<Double>(data.Cast<Double>());
            return new Column<String>(data.Cast<String>());
        }

        public static IColumn CreateColumn(DataColumn dc, IEnumerable<object> data)
        {
            if (data == null)
                return CreateColumn(dc.DataType, new List<Object>()).Set(c => c.Name = dc.ColumnName);
            return CreateColumn(dc.DataType, data).Set(c => c.Name = dc.ColumnName);
        }

        /// <summary>
        /// Creates a column from base column. Filter is a list of ints that represents indexes of data in col
        /// </summary>
        public static IColumn CreateColumn(IColumn baseCol, IEnumerable<int> filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");
            if (!baseCol.GetType().IsGenericType)
                throw new InvalidOperationException("Passed IColumn is not of type Column<T>");
            var colType = baseCol.GetType().GetGenericArguments()[0];
            return CreateColumn(colType, filter.Select(i => baseCol[i]).OrderBy(i => i)).Set(c => c.Name = baseCol.Name);
        }

        public static object CreateTable(ITable iTable, IEnumerable<IRow> allRows)
        {
            throw new NotImplementedException();
        }
    }
}
