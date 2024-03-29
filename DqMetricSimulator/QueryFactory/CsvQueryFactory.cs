﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using DqMetricSimulator.IO;
using DqMetricSimulator.Query;

namespace DqMetricSimulator.QueryFactory
{
    public class CsvQueryFactory : QueryFactoryBase
    {
        public CsvQueryFactory(IIoProvider ioProvider)
        {
            _ioProvider = ioProvider;
        }

        public override IEnumerable<IQuery> Create(IDictionary<string, string> options)
        {
            CheckOption(options, "FileName", "Table");
            var fileName = options["FileName"];
            var tableName = options["Table"];
            var metaCols = options.Where(o => o.Key.StartsWith("Meta")).Select(o => o.Value.ToLowerInvariant()).ToList();

            //Read the file as csv
            var data = ReadTableFromCsv(fileName);
            var projections = ProjectionsFromTable(data, metaCols);
            var queriesWithMeta =
                data.Rows.Cast<DataRow>().Select(dr =>
                    Tuple.Create(
                                                 (IQuery) new BasicQuery(projections,
                                                                dr.ItemArray.Select((o, i) => new {o, i}).
                                                                    Where(oi => oi.o != DBNull.Value &&
                                                                                !metaCols.Contains(
                                                                                    data.Columns[oi.i].ColumnName.ToLowerInvariant())).
                                                                    Select(
                                                                        oi =>
                                                                        EqualitySelectionConditionFromColumn(
                                                                            dr.Table.Columns[oi.i], oi.o)),
                                                                new[] {tableName}
                                                     ),
                                                     ExtractMetadata(dr, metaCols)));
            _queriesWithMeta = queriesWithMeta;
            return queriesWithMeta.Select(q => q.Item1);
        }

        private static object[] ExtractMetadata(DataRow dr, IEnumerable<string> metaCols)
        {
            return dr.ItemArray.Select((v, i) => metaCols.Contains(dr.Table.Columns[i].ColumnName.ToLowerInvariant()) ? v : null).Where(
                v => v != null).ToArray();
        }

        private static void CheckOption(IDictionary<string, string> options, params string[] names)
        {
            foreach (var name in names.Where(name => !options.ContainsKey(name)))
            {
                throw new ArgumentException(String.Format("An option '{0}' must be passed to the Csv importer.", name ));
            }
        }

        private static IEnumerable<IProjection> ProjectionsFromTable(DataTable data, IEnumerable<string> metaCos)
        {
            return (from DataColumn column in data.Columns
                    where !metaCos.Contains(column.ColumnName.ToLowerInvariant())
                    select ProjectionItem.CreateFromName(column.ColumnName, false, column.DataType)).ToList();
        }

        private static ISelectionCondition EqualitySelectionConditionFromColumn(DataColumn dataColumn, object o)
        {
            var dataType = dataColumn.DataType;
            var col = dataColumn.ColumnName;
            if (dataType == typeof(string))
                return SelectionCondition.Equality(col, (string) o);
            if (dataType == typeof(int))
                return SelectionCondition.Equality(col, (int) o);
            throw new InvalidOperationException(String.Format("Data type {0} is not supported.", dataType));
        }

        private static IIoProvider _ioProvider;
        private const string Dellimiter = @"<!@#!@#,~12(}>";
        private static readonly string[] Types = new[] {":string", ":int"};
        private static readonly Dictionary<string, Type> TypeTranslate = new Dictionary<string, Type>
                                                                     {
                                                                         {":string", typeof(string)},
                                                                         {":int", typeof(int)}
                                                                     };
        private static readonly Regex TextFinder = new Regex(@"""?,\s*""?");
        private IEnumerable<Tuple<IQuery, object[]>> _queriesWithMeta;

        public IEnumerable<Tuple<IQuery, object[]>> QueriesWithMeta
        {
            get { return _queriesWithMeta; }
        }

        private static DataTable ReadTableFromCsv(string fileName)
        {
            _ioProvider.SetSource(fileName);
            var allLines = _ioProvider.ReadAllLines();
            if (allLines.Length < 2)
            {
                throw new ArgumentException(String.Format("File '{0}' has no data.", fileName));
            }
            var headers = RegexSplit(allLines[0]);
            var data = allLines.Where((l, i) => i > 0).Select(RegexSplit);
            var table = new DataTable();
            table.Columns.AddRange(headers.Select(GetDataColumnWithType).ToArray());
            data.ToList().ForEach(r =>
                                      {
                                          var dr = table.NewRow();
                                          dr.ItemArray =
                                              r.Select((o, i) => o == "" ? DBNull.Value : Convert.ChangeType(o, table.Columns[i].DataType)).
                                                  ToArray();
                                          table.Rows.Add(dr);
                                      });
            return table;
        }

        private static DataColumn GetDataColumnWithType(string col)
        {
            var type = Types.FirstOrDefault(col.EndsWith);
            return type != null ? CreateDataColumn(col.Substring(0, col.Length - type.Length), type) : CreateDataColumn(col, ":string");
        }

        private static DataColumn CreateDataColumn(string col, string type)
        {
            return new DataColumn(col) {DataType = TypeTranslate[type]};
        }

        private static string[] RegexSplit(string data)
        {
            return TextFinder.Replace(data, Dellimiter).Split(new[] {Dellimiter}, StringSplitOptions.None);
        }
    }
}
