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
    class DblpSamples
    {

        private static IEnumerable<IQuery> RangesForNaive()
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

            var queries = GetAllQueries(merged, "productId", "color");
            return queries;

        }


        public static void ExecuteTestForNaiveApproach(String fileName)
        {
            //Now enter queries into system and run them all.
            var queries = RangesForNaive();
            var toOut = RunTest(queries, 0.7);
            System.IO.File.WriteAllText(fileName, toOut);
        }


        private static string RunTest(IEnumerable<IQuery> queries, double confidenceThreshold)
        {
            var ds = new SimpleSqlDataService("Data Source=.; Initial Catalog=AdventureWorksLT2008; Integrated Security=SSPI");
            var qas = new SimpleQueryAnsweringService(ds);
            var cs = new NaiveCostService();
            var dqs = new SuperSimpleDqService();
            var context = new SapmlingContext(qas, cs, ds, dqs);
            context.Initialize();
            var outputRes = new StringBuilder();
            outputRes.AppendLine("Q#, TimeTotal, SampleSize, QueryFromSample, ConfidenceThreshold");
            var qNumber = 0;
            var timeTotal = default(long);
            var sampleSize = 0;
            var queryFromSample = 0;
            foreach (var query in queries)
            {
                ITable result;
                var pre = DateTime.Now;
                var ts = TimeSpan.MinValue;
                context.PreQueryRunEvent = q => { ts = DateTime.Now - pre; };
                context.PostQueryRunEvent = q => { pre = DateTime.Now; };
                context.SampleMaterialzied = (q, sample) => { sampleSize += sample.Table.Rows.Count; };
                
                var est = context.ExecuteQuery(query, out result);
                qNumber++;
                timeTotal += ts.Ticks;
                if (est != null && est.Confidence > confidenceThreshold)
                    queryFromSample++;
            }

            outputRes.AppendFormat("{0},{1},{2},{3},{4}", qNumber, timeTotal, sampleSize, queryFromSample,
                                   confidenceThreshold);
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

        private static IEnumerable<IQuery> GetAllQueries(IEnumerable<Tuple<string, ISelectionCondition>> reference, params string[] columns)
        {
            var allConds = GetAllConditions(reference, null, 0, columns);
            var allCondsOrdered = allConds.OrderBy(a => a.Count);
            return allCondsOrdered.Select(QueryGenerator.GetQuery).ToList();
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
