using System;
using System.Collections.Generic;
using System.Linq;
using AlgebraToSqlServer;
using AlgebraTree;
using DqMetricSimulator.Core;
using DqMetricSimulator.Dq;
using DqMetricSimulator.IO;
using DqMetricSimulator.Query;
using DqMetricSimulator.QueryFactory;

namespace Evaluation
{
    public class EvaluateImportedQueries
    {
        public static readonly string Titles =
            //"queries, totalTime, sampleCount, sampleSize, queryFromSample, confidenceThr, memLimit, samplingRate, minimumError, maximumError, naive, basic, costModelPreKnown, costModelUnknown, optimized";
            "queries, totalTime, confidenceThr, memLimit, samplingRate" +
            ", sc_naive, ss_naive, qfs_naive, me_naive, Me_naive, er_naive" +
            ", sc_basic, ss_basic, qfs_basic, me_basic, Me_basic, er_basic" +
            ", sc_costk, ss_costk, qfs_costk, me_costk, Me_costk, er_costk" +
            ", sc_costu, ss_costu, qfs_costu, me_costu, Me_costu, er_costu" +
            ", sc_optim, ss_optim, qfs_optim, me_optim, Me_optim, er_optim";
            

        private string _server;
        private string _database;
        private string _table;
        private CostParameters _costs;
        private string _dqColumn;
        private IQuery[] _queries;
        private List<PreKnownQuery> _preKnownQueries;

        private string ConnectionString
        {
            get { return String.Format("Database={0}; Initial Catalog={1}; Integrated Security=SSPI", _server, _database); }
        }

        public void Initialize(string fileName, string server, string database, string table, string dqColumn, string popularityCol, string costCol, CostParameters costs)
        {
            _server = server;
            _database = database;
            _table = table;
            _costs = costs;
            _dqColumn = dqColumn;
            LoadQueries(fileName, popularityCol, costCol);
        }

        /// <summary>
        /// Runs all algorithms and returns the results in a tabluar format
        /// </summary>
        /// <returns></returns>
        public double[] RunEvaluation()
        {
            var naiveResult = RunNaive();
            var basicResult = RunBasic();
            var costPreKnown = RunPreKnownCostBased();
            var costUnknown = RunUnknownCostBased();
            var costOptimized = costPreKnown;// RunOptimizedCostBased();

            const int costCols = 5;
            return naiveResult.Take(costCols)
                .Concat(naiveResult.Skip(costCols))
                .Concat(basicResult.Skip(costCols))
                .Concat(costPreKnown.Skip(costCols))
                .Concat(costUnknown.Skip(costCols))
                .Concat(costOptimized.Skip(costCols))
                .ToArray();
        }

        private void LoadQueries(string fileName, string popCol, string costCol)
        {
            var qfactory = new CsvQueryFactory(new FileIoProvider());
            _queries = qfactory.Create(new Dictionary<string, string> {{"FileName", fileName}, {"Table", _table}, {"Meta1", popCol}, {"Meta2", costCol}}).ToArray();
            _preKnownQueries =
                qfactory.QueriesWithMeta.Select(qwm => GetPreKnownQueryFromMeta(qwm.Item1, qwm.Item2)).ToList();
        }

        private static PreKnownQuery GetPreKnownQueryFromMeta(IQuery query, IList<object> meta)
        {
            if (meta.Count != 2)
                throw new InvalidOperationException(@"The query is expected to have two metadata colums: Popularity and Cost");
            return new PreKnownQuery
                       {
                           Query = query,
                           Popularity = Convert.ToInt32(meta[0]),
                           Cost = Convert.ToInt32(meta[1]),
                       };
        }

        private double[] RunBasic(float ? overrideConfidence = null)
        {
            var ds = new SimpleSqlDataService(ConnectionString);
            var qas = new SimpleQueryAnsweringService(ds);
            var cs = new NaiveCostService(_costs.Size, long.MaxValue);
            var dqs = new SuperSimpleDqService();
            var context = new SapmlingContext(qas, cs, ds, dqs) {SamplingRate = _costs.BaseSamplingRate, ConfidenceThreshold = overrideConfidence.HasValue ? overrideConfidence.Value : _costs.Confidence};
            return RunTest(context, _queries, _costs.Size,  _costs.BaseSamplingRate, _dqColumn);
        }

        private double[] RunOptimizedCostBased()
        {
            var ds = new SimpleSqlDataService(ConnectionString);
            var qas = new OptimizedQueryAnsweringService(ds);
            var cs = new UnknownLogCostService(_costs.Size, 100);
            var dqs = new SuperSimpleDqService();
            var context = new SapmlingContext(qas, cs, ds, dqs) {SamplingRate = _costs.BaseSamplingRate, ConfidenceThreshold = _costs.Confidence};
            return RunTest(context, _queries, _costs.Size, _costs.BaseSamplingRate, _dqColumn);
        }

        private double[] RunUnknownCostBased()
        {
            var ds = new SimpleSqlDataService(ConnectionString);
            var qas = new SimpleQueryAnsweringService(ds);
            var cs = new UnknownLogCostService((float)_costs.Size/1600, 30);
            var dqs = new SuperSimpleDqService();
            var context = new SapmlingContext(qas, cs, ds, dqs) {SamplingRate = _costs.BaseSamplingRate, ConfidenceThreshold = _costs.Confidence};
            return RunTest(context, _queries, _costs.Size, _costs.BaseSamplingRate, _dqColumn);
        }

        private  double[] RunPreKnownCostBased()
        {
            var ds = new SimpleSqlDataService(ConnectionString);
            var qas = new SimpleQueryAnsweringService(ds);
            var cs = new PreKnownLogCostService(_costs.Size, _preKnownQueries);
            var dqs = new SuperSimpleDqService();
            var context = new SapmlingContext(qas, cs, ds, dqs) {SamplingRate = _costs.BaseSamplingRate, ConfidenceThreshold = _costs.Confidence};
            return RunTest(context, _queries, _costs.Size, _costs.BaseSamplingRate, _dqColumn);
        }

        private  double[] RunNaive()
        {
            return RunBasic(1.5f);
        }

        private static double[] RunTest(SapmlingContext context, IEnumerable<IQuery> queries, long memLimit, float samplingRate, string dqColumn)
        {
            context.Initialize();
            var qNumber = 0;
            var timeTotal = default(long);
            var sampleSize = 0;
            var sampleCount = 0;
            var queryFromSample = 0;
            double minEr = Double.MaxValue, maxEr = Double.MinValue, sumEr = 0.0d;
            var pre = new DateTime();
            var ts=new TimeSpan();
            context.PreQueryRunEvent = q => { ts = DateTime.Now - pre; };
            context.PostQueryRunEvent = q => { pre = DateTime.Now; };
            context.SampleMaterialzied = (q, sample) =>
                                             {
                                                 sampleCount++; sampleSize += sample.Table.Rows.Count; };
            foreach (var query in queries)
            {
                ITable result;
                pre = DateTime.Now;
                
                var est = context.ExecuteQuery(query, out result);
                qNumber++;
                timeTotal += ts.Ticks;
                if (est != null && est.Confidence > context.ConfidenceThreshold)
                {
                    queryFromSample++;
                    var estimatedQuality = (double)est.Result.Rows.Sum(r => int.Parse((string)est.Result.GetValueByColumn(dqColumn, r)))
                                           /est.Result.Rows.Count;
                    var actualQuality = (double) result.Rows.Sum(r => int.Parse((string)result.GetValueByColumn(dqColumn, r)))
                                        /result.Rows.Count;
                    var er = Math.Abs(estimatedQuality - actualQuality);
                    minEr = Math.Min(minEr, er);
                    maxEr = Math.Max(maxEr, er);
                    sumEr += er;
                }
            }

            return new[]
                       {
                           qNumber, timeTotal, context.ConfidenceThreshold, memLimit, samplingRate,
                           sampleCount, sampleSize, queryFromSample, minEr
                           , maxEr, sumEr/Math.Max(queryFromSample,1)
                       };

            //Keep record of expenses: Qid, TimeToExecute
            //Keep record of size: Qid, SampleSize and creation costs if new sample materialzied.
            //Graph out above numbers.
        }
    }
}
