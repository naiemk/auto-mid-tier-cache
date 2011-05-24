using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlgebraToSqlServer;
using AlgebraTree;
using DqMetricSimulator.Core;
using DqMetricSimulator.Dq;
using DqMetricSimulator.Query;

namespace Evaluation
{

    public static class MyAddons
    {
        public static QueryGenerator PaperIdRange(this QueryGenerator qg, int start, int len)
        {
            var sl = start + len;
            return qg.Range<Int64>(paperId => paperId > start, paperId => paperId < sl);
        }
    }

    class DblpSamples
    {

        public static IEnumerable<IQuery> RangesForNaiveDblp()
        {
            var ranges = new QueryGenerator()
                .PaperIdRange(0, 6000)
                .PaperIdRange(0, 1000)
                .PaperIdRange(1000, 1000)
                .PaperIdRange(2000, 1000)
                .PaperIdRange(0, 500)
                .PaperIdRange(500, 500)
                .PaperIdRange(1000, 500)
                .PaperIdRange(1500, 500)
                .PaperIdRange(2000, 500)
                .PaperIdRange(2500, 500)
                .PaperIdRange(3000, 500)
                .PaperIdRange(0, 100)
                .PaperIdRange(100, 100)
                .PaperIdRange(200, 100)
                .PaperIdRange(300, 100)
                .PaperIdRange(400, 100)
                .PaperIdRange(500, 100)
                .PaperIdRange(600, 100)
                .PaperIdRange(700, 100)
                .PaperIdRange(800, 100)
                .PaperIdRange(900, 100)
                .PaperIdRange(1000, 100)
                .PaperIdRange(1100, 100)
                .PaperIdRange(1200, 100)
                .PaperIdRange(1300, 100)
                .PaperIdRange(1400, 100)
                .PaperIdRange(1500, 100)
                .PaperIdRange(1600, 100)
                .PaperIdRange(1700, 100)
                .PaperIdRange(1800, 100)
                .PaperIdRange(1900, 100)
                .PaperIdRange(1000, 20)
                .PaperIdRange(1040, 20)
                .PaperIdRange(1080, 20)
                .PaperIdRange(1100, 20)
                .PaperIdRange(1140, 20)
                .PaperIdRange(1180, 20)
                .PaperIdRange(1200, 20)
                .Range<Int64>(conferenceId => conferenceId > 100, conferenceId => conferenceId < 200)
                .Range<Int64>(conferenceId => conferenceId > 200, conferenceId => conferenceId < 300)
                .Range<Int64>(conferenceId => conferenceId > 300, conferenceId => conferenceId < 400)
                .Range<Int64>(conferenceId => conferenceId > 300, conferenceId => conferenceId < 350)
                ;

            ranges.
            Projections = new[]
                {
                    ProjectionItem.CreateFromName<Int64>("PaperId", true),
                    ProjectionItem.CreateFromName<Int64>("ConferenceId", false),
                    ProjectionItem.CreateFromName<String>("Paper", false),
                    ProjectionItem.CreateFromName<String>("Proceeding", false),
                    ProjectionItem.CreateFromBasicMetric("m_Completeness", "Paper"),
                    ProjectionItem.CreateFromBasicMetric("m_Correctness", "Paper")
                }.ToList();
            ranges.Table = "Temp_PaperConferences";

            var queries1 = GetAllQueries(ranges, ranges.Conds, "paperId");
            var queries2 = GetAllQueries(ranges, ranges.Conds, "conferenceId");

            return queries1;
        }

        public static IEnumerable<IQuery> RangesForNaiveAw()
        {
            //Ranges
            var ranges = new QueryGenerator()
                .Range<Int32>(productId => productId > 0, productId => productId < 10)
                .Range<Int32>(productId => productId > 5, productId => productId < 15)
                .Range<Int32>(productId => productId > 10, productId => productId < 20)
                .Range<Int32>(productId => productId > 15, productId => productId < 25)
                .Range<Int32>(productId => productId > 20, productId => productId < 30)
                .Range<Int32>(productId => productId > 25, productId => productId < 35)
                .Range<Int32>(productId => productId > 30, productId => productId < 40)
                .Range<Int32>(productId => productId > 35, productId => productId < 45);

            //Equalities
            var consts = new QueryGenerator()
                .Add<String>(color => color == "Black")
                .Add<String>(color => color == "Green")
                .Add<String>(color => color == "Red")
                .Add<String>(color => color == "Brown")
                .Add<String>(color => color == "Blue")
                .Add<String>(color => color == "Pink")
                .Add<String>(color => color == "White");

            //Merge lists.
            var merged = MergeLists(ranges, consts, 0.7);

            var queries = GetAllQueries(ranges, merged, "productId", "color");
            return queries;

        }


        public static string ExecuteTestForNaiveApproach(string connStr, IEnumerable<IQuery> queries, float confThresh, long queryLimit, long memLimit, bool includeHeader)
        {
            //Now enter queries into system and run them all.
            return RunTestNaive(queries, confThresh, connStr, queryLimit, memLimit, includeHeader);
        }

        public static string RunTestBasic(IEnumerable<IQuery> queries, float confidenceThreshold, String connStr, long queryLimit, long memLimit, bool includeHeader)
        {
            var ds = new SimpleSqlDataService(connStr);
            var qas = new SimpleQueryAnsweringService(ds);
            var cs = new BasicCostService(queryLimit, memLimit, queries.Count(), 720);
            var dqs = new SuperSimpleDqService();
            var context = new SapmlingContext(qas, cs, ds, dqs) {SamplingRate = 0.05f, ConfidenceThreshold = confidenceThreshold};
            return RunTest(context, queries, queryLimit, memLimit, includeHeader);
        }

        private static string RunTestNaive(IEnumerable<IQuery> queries, float confidenceThreshold, String connStr, long queryLimit, long memLimit, bool includeHeader)
        {
            var ds = new SimpleSqlDataService(connStr);
            var qas = new SimpleQueryAnsweringService(ds);
            var cs = new NaiveCostService(memLimit, queryLimit);
            var dqs = new SuperSimpleDqService();
            var context = new SapmlingContext(qas, cs, ds, dqs) {SamplingRate = 0.05f, ConfidenceThreshold = confidenceThreshold};
            return RunTest(context, queries, queryLimit, memLimit, includeHeader);
        }

        private static string RunTest(SapmlingContext context, IEnumerable<IQuery> queries, long queryLimit, long memLimit, bool includeHeader)
        {
            context.Initialize();
            var outputRes = new StringBuilder();
            if (includeHeader) outputRes.AppendLine("Q#, TimeTotal, SampleCount, SampleSize, QueryFromSample, ConfidenceThreshold, maxQ, memLimit");
            var qNumber = 0;
            var timeTotal = default(long);
            var sampleSize = 0;
            var sampleCount = 0;
            var queryFromSample = 0;
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
                    queryFromSample++;
            }

            outputRes.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}", qNumber, timeTotal, sampleCount, sampleSize, queryFromSample,
                                   context.ConfidenceThreshold, queryLimit, memLimit );
            outputRes.AppendLine();

            //Keep record of expenses: Qid, TimeToExecute
            //Keep record of size: Qid, SampleSize and creation costs if new sample materialzied.
            //Graph out above numbers.
            return outputRes.ToString();
            
        }

        private static IEnumerable<Tuple<string, ISelectionCondition>> MergeLists(QueryGenerator ranges, QueryGenerator consts, double rate)
        {
            var rLen = (int)(ranges.Conds.Count*rate);
            var cLen = (int)(consts.Conds.Count*(1 - rate));
            var rv = new List<Tuple<String, ISelectionCondition>>( ranges.Conds.Take(rLen) );
            rv.AddRange( consts.Conds.Take(cLen));
            return rv;
        }

        private static IEnumerable<IQuery> GetAllQueries(QueryGenerator qg, IEnumerable<Tuple<string, ISelectionCondition>> reference, params string[] columns)
        {
            var allConds = GetAllConditions(reference, null, 0, columns);
            var allCondsOrdered = allConds.OrderBy(a => a.Count);
            return allCondsOrdered.Select(qg.GetQuery).ToList();
        }

        private static List<List<ISelectionCondition>> GetAllConditions(IEnumerable<Tuple<string, ISelectionCondition>> reference, 
            List<List<ISelectionCondition>> baseList, int index, params string[] columns)
        {
            if (index < columns.Length-1)
            {
                baseList = GetAllConditions(reference, baseList, index + 1, columns);
            }
            else
            {
                //Create base list from the last column
                return
                    reference.Where(r => r.Item1 == columns.Last()).Select(
                        s => new List<ISelectionCondition>(new[] {s.Item2})).ToList();
            }
            var newBaseList = new List<List<ISelectionCondition>>(baseList);
            foreach (var list in baseList)
            {
                reference.Where(r => r.Item1 == columns[index]).ToList().ForEach(l =>
                                                                                     {
                                                                                        var newSubList = new List<ISelectionCondition>();
                                                                                        newSubList.AddRange(list);
                                                                                        newSubList.Add(l.Item2);
                                                                                        newBaseList.Add(newSubList);
                                                                                     });
            }
            return newBaseList;
        }
    }
}
