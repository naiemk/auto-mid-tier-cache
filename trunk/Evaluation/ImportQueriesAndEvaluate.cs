using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Common.Data.Sql;

namespace Evaluation
{
    internal class ImportQueriesAndEvaluate
    {
        private string _server = ".";
        private string _database = "dblp";
        private string _table = "DblpData";
        private string _dqColumn = "dq";
        private string _popularityColumn = "Popularity";
        private string _costColumn = "Cost";
        private static ILogger _logger = new DebugLogger();
        private Dal _dal = new Dal(_logger, new Progress(_logger, 10));
        private float _coverage = 0.9f;
        private string _outFolder = @"C:\NKProg\JisEval\";
        private float _samplingRate = 0.05f;

        private string ConnectionString
        {
            get { return String.Format("Database={0}; Initial Catalog={1}; Integrated Security=SSPI", _server, _database); }
        }

        /// <summary>
        /// Executes the test. Assumes data availability.
        /// </summary>
        public void ExecuteForDifferentCostLimit()
        {
            var rv = _dal.GetDataSet(ConnectionString, String.Format(" exec GenerateQueries {0}, {1}", _coverage, 10000));
            var actualCoverage = (float)(decimal)rv.Tables[0].Rows[0]["coverage"];

            var data = ExportToCsv(rv.Tables[1]);
            var outFile = Path.Combine(_outFolder, String.Format("Data{0}.csv", new Guid()));
            File.WriteAllText(outFile, data);

            var outResultFile = Path.Combine(_outFolder, String.Format("Result{0}.csv", new Guid()));
            var sb = new StringBuilder(String.Format("{0},Coverage={1}", EvaluateImportedQueries.Titles, actualCoverage));
            sb.AppendLine();
            //Run for different cost limits
            for (var size = 100; size < 10000; size += 1000)
            {
                var evaler = new EvaluateImportedQueries();
                evaler.Initialize(outFile, _server, _database, _table, _dqColumn, _popularityColumn, _costColumn,
                                  new CostParameters
                                      {
                                          BaseSamplingRate = _samplingRate,
                                          Confidence = 0.5f,
                                          Size = size,
                                      }
                    );
                var res = evaler.RunEvaluation();
                sb.AppendLine(JoinStr(",", res.Select(r => r.ToString())));
            }
            File.WriteAllText(outResultFile, sb.ToString());
        }

        public void ExecuteForDifferentCoverage()
        {
            for(_coverage = 0.1f; _coverage < 0.9; _coverage += 0.15f)
            {
                ExecuteForDifferentCostLimit();
            }
        }

        public void ExecuteForDifferentSamplingRate()
        {
            for(_samplingRate = 0.01f; _samplingRate < 0.1f; _samplingRate += 0.1f)
            {
                ExecuteForDifferentCostLimit();
            }
        }

        private static string ExportToCsv(DataTable table)
        {
            var sb = new StringBuilder();
            sb.AppendLine(JoinStr(",", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));

            foreach (DataRow row in table.Rows)
            {
                sb.AppendLine(JoinStr(",", row.ItemArray.Select(i => String.Format("{0}", i))));
            } 
            return sb.ToString();
        }

        private static string JoinStr(string delim, IEnumerable<string> items)
        {
            var rv = new StringBuilder();
            var notFirst = false;
            foreach (var item in items)
            {
                if (notFirst)
                    rv.Append(delim);
                rv.Append(item.Contains(delim) ? String.Format("\"{0}\"", item) : item);
                notFirst = true;
            }
            return rv.ToString();
        }
    }
}
