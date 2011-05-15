using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Common;
using Common.Data.Sql;
using DqMetricSimulator.Core;
using DqMetricSimulator.Data;
using DqMetricSimulator.Simulate;
using DqMetricSimulator.Statistics;

namespace DblpDqSimulator
{
    public partial class frmGen : Form
    {
        private const double SubsetLevelRatio = 0.15;

        public frmGen()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            var container = Bootstrapper.Get();
            var dal = container.Resolve<Dal>();

            const string query = @"
SELECT  p.Id, r.Id, p.title, r.conference, r.title, r.publisher, r.year
FROM papers p
INNER JOIN Raw_Links l
ON p.Id = l.[From]
INNER JOIN proceedings r
ON l.[To] = r.Id
AND l.[link-type]='in-proceedings'
";
            //Select all conferences.
            var dt = dal.GetDataTable(Dal.DefaultConnStr, query);
            dt.PrimaryKey = new[] {dt.Columns[0], dt.Columns[1]}; // Setting the primary key is necessary
            var tbl = ImportDataTable.ImportAdoNetDataTable(dt);
            var baseHist = SimulationBuilder.GetNormalDistributedBaseHistogram();
            
            //Now we have the base histogram.
            //We want to generate a histogram for a column and save it back to database.
            //Let's select conference
            const int selectedCol = 3;
            const int metricId = 1;
            var hist = SimulationBuilder.GetHistogram(tbl.Columns[selectedCol] as Column<string>, baseHist);
            var time = new Random(DateTime.Now.Second);
            var loockup = SimulationBuilder.CreateSimulatedLoockup(tbl, selectedCol, hist,
                                                              r => TimeSpan.FromMilliseconds(time.Next(3000)));
            //Now we've got the loockup. Store it somewehere. We put it back in the database.
            //We already have a table called SimulatedMetricLoockup
            const string insertMetric = "INSERT INTO Metrics (Id, MetricName, OnQuery, OnColumn) VALUES (@mId, @mName, @mQuery, @mCol);";
            const string updateMetric = "UPDATE Metrics SET Id = @mId, MetricName = @mName, OnQuery = @mQuery, OnColumn =  @mCol";
            dal.InsertOrUpdateEntity(Dal.DefaultConnStr,
                                     insertMetric,
                                     updateMetric,
                                     "SELECT Count(*) FROM Metrics Where Id = @mId",
                                     new Dictionary<string, object>
                                         {
                                             {"@mId", metricId},
                                             {"@mName", "NormalDistru"},
                                             {"mQuery", query},
                                             {"mCol", selectedCol}
                                         }
                );

            //Put everything in a temp file.
            var tmpFileName = dal.InsertAllInTempFile(loockup, l => String.Format("{0},{1},{2},{3}", metricId, l.Key, l.Value.Result ? 1 : 0, l.Value.Duration.Milliseconds) );
            //bulk insert 
            string bulkIns =
                @"BULK INSERT SimulatedMetricLoockup FROM '{0}' WITH ( FIELDTERMINATOR = ',', ROWTERMINATOR='\n' )".
                    FormatWith(tmpFileName);
            dal.RunCommandWithParameter(Dal.DefaultConnStr, bulkIns);
            //const string insertLoockupQ = @"INSERT INTO SimulatedMetricLoockup (MetricId, LoockupKey, LoockupValue, Delay) VALUES (@mId, @lKey, @lVal, @lD)";
            //dal.RunForAll(Dal.DefaultConnStr, insertLoockupQ, loockup.ToList(),
            //              l => new Dictionary<string, object> {{"@mId", 1}, {"@lKey", l.Key}, {"@lVal", l.Value.Result}, {"@lD", l.Value.Duration}}
            //    );
        }

        public void RandomizeWithHierarchy()
        {
            //Generate histogram as follows:
            //Assume a list of columns (e.g. COnference, Publisher, Year) ordred by set size descending
            //Generate histogram for Conference 
            //Generate result for it (as base)
            //a.Select a subset of conference AND a subset of Publisher (Subset ratio e.g. 15%)
            //Filter base data and generate result for it
            //Apply the results back to the base result (i.e. join back)
            //Repeat from a. for the rest.
            //See what happens.


            var container = Bootstrapper.Get();
            var dal = container.Resolve<Dal>();
            var logger = container.Resolve<ILogger>();
            var progresser = container.Resolve<IProgress>();

            const string query = @"
SELECT  p.Id, r.Id, p.title, r.conference, r.title, r.publisher, r.year
FROM papers p
INNER JOIN Raw_Links l
ON p.Id = l.[From]
INNER JOIN proceedings r
ON l.[To] = r.Id
AND l.[link-type]='in-proceedings'
";
            //Select all conferences.
            var dt = dal.GetDataTable(Dal.DefaultConnStr, query);
            dt.PrimaryKey = new[] {dt.Columns[0], dt.Columns[1]}; // Setting the primary key is necessary
            var tbl = ImportDataTable.ImportAdoNetDataTable(dt);
            var baseHist = SimulationBuilder.GetNormalDistributedBaseHistogram();
            
            //Now we have the base histogram.
            //We want to generate a histogram for a column and save it back to database.
            //Let's select conference
            var selectedCols = new[] { 3, 5, 6 };
            const int metricId = 1;
            var hist = SimulationBuilder.GetHistogram(tbl.Columns[selectedCols[0]] as Column<string>, baseHist);
            var time = new Random(DateTime.Now.Second);
            var baseLoockup = SimulationBuilder.CreateSimulatedLoockup(tbl, selectedCols[0], hist,
                                                              r => TimeSpan.FromMilliseconds(time.Next(3000)));


            using (var fList = System.IO.File.CreateText(@"C:\Differents.csv"))
            {
                //Select subset of this column and subset of the next one.
                var colIdx = 0;
                var filteredData = tbl.Rows;
                while (colIdx < selectedCols.Length - 1)
                {
                    var col1 = selectedCols[colIdx];
                    var col2 = selectedCols[colIdx + 1];
                    colIdx++;
                    var col1Items = GetSubset(tbl.Columns[col1], SubsetLevelRatio);
                    var col2Items = GetSubset(tbl.Columns[col2], SubsetLevelRatio);
                    filteredData =
                        filteredData.Where(r => col1Items.Contains(r.Rows[col1]) && col2Items.Contains(r.Rows[col2])).
                            Select(
                                r => r).ToList();

                    fList.WriteLine("NewList, {0}, {1}", col1, col2);
                    filteredData.Select(
                        f => new {col1 = tbl.Columns[col1][f.Rows[col1]], col2 = tbl.Columns[col2][f.Rows[col2]]})
                        .Distinct().ToList().ForEach(l => fList.WriteLine("{0},{1}", l.col1, l.col2));

                    logger.Log("Filtered items to {0} records now replacing it back.".FormatWith(filteredData.Count));
                    progresser.Reset(baseLoockup.Count);

                    //replace baseLoockup with filtered data.
                    var newLoockup = SimulationBuilder.CreateSimulatedLoockup(tbl, filteredData, selectedCols[0], hist,
                                                                              r =>
                                                                              TimeSpan.FromMilliseconds(time.Next(3000)));
                    foreach (var item in baseLoockup.Where(l => newLoockup.ContainsKey(l.Key)))
                    {
                        progresser.Progressed(1);
                        var newItem = newLoockup[item.Key];
                        item.Value.Result = newItem.Result;
                        item.Value.Duration = newItem.Duration;
                    }
                    progresser.Finish();
                }
                fList.Close();
            }

            //Now we've got the loockup. Store it somewehere. We put it back in the database.
            //We already have a table called SimulatedMetricLoockup
            const string insertMetric = "INSERT INTO Metrics (Id, MetricName, OnQuery, OnColumn) VALUES (@mId, @mName, @mQuery, @mCol);";
            const string updateMetric = "UPDATE Metrics SET Id = @mId, MetricName = @mName, OnQuery = @mQuery, OnColumn =  @mCol";
            dal.InsertOrUpdateEntity(Dal.DefaultConnStr,
                                     insertMetric,
                                     updateMetric,
                                     "SELECT Count(*) FROM Metrics Where Id = @mId",
                                     new Dictionary<string, object>
                                         {
                                             {"@mId", metricId},
                                             {"@mName", "NormalDistru"},
                                             {"mQuery", query},
                                             {"mCol", selectedCols[0]}
                                         }
                );

            //Put everything in a temp file.
            var tmpFileName = dal.InsertAllInTempFile(baseLoockup, l => String.Format("{0},{1},{2},{3}", metricId, l.Key, l.Value.Result ? 1 : 0, l.Value.Duration.Milliseconds) );
            //bulk insert 
            string bulkIns =
                @"BULK INSERT SimulatedMetricLoockup FROM '{0}' WITH ( FIELDTERMINATOR = ',', ROWTERMINATOR='\n' )".
                    FormatWith(tmpFileName);
            dal.RunCommandWithParameter(Dal.DefaultConnStr, bulkIns);
        }

        /// <summary>
        /// Returns a random subset of a column. It does not return actual values, but the index of objects in the column.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="subsetLevelRatio"></param>
        /// <returns></returns>
        private static HashSet<int> GetSubset(IColumn column, double subsetLevelRatio)
        {
            var subsetSize = (int)(column.Count*subsetLevelRatio);
            var random = new Random(DateTime.Now.Second);
            var rv = Utils.Sequence(0, column.Count).Select(i => new {i, r = random.NextDouble()}).OrderBy(t => t.r)
                .Select(t => t.i).Take(subsetSize);
            var hs = new HashSet<int>();
            return rv.ToHashSet();
        }

        private void btnRubHierarchial_Click(object sender, EventArgs e)
        {
            RandomizeWithHierarchy();
        }
    }
}
