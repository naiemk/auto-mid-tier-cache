using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DqMetricSimulator.Query;

namespace Evaluation
{
    class DblpSamples
    {

        public static List<IQuery> Ranges()
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

            var queries = GetAllQueries(merged, "proructId", "color");

            //Now enter queries into system and run them all.
            //Keep record of expenses: Qid, TimeToExecute
            //Keep record of size: Qid, SampleSize and creation costs if new sample materialzied.
            //Graph out above numbers.
        }

        private static List<Tuple<String, ISelectionCondition>> MergeLists(QueryGenerator ranges, QueryGenerator consts, double rate)
        {
            var rLen = (int)(ranges.Conds.Count*rate);
            var cLen = (int)(consts.Conds.Count*(1 - rate));
            var rv = new List<Tuple<String, ISelectionCondition>>( ranges.Conds.Take(rLen) );
            rv.AddRange( consts.Conds.Take(cLen));
            return rv;
        }

        public static List<IQuery> GetAllQueries(List<Tuple<String, ISelectionCondition>> reference, params string[] columns)
        {
            var allConds = GetAllConditions(reference, null, 0, columns);
            var allCondsOrdered = allConds.OrderBy(a => a.Count);
            return allCondsOrdered.Select(QueryGenerator.GetQuery).ToList();
        }

        public static List<List<ISelectionCondition>> GetAllConditions(List<Tuple<String, ISelectionCondition>> reference, 
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
            foreach (var list in baseList)
            {
                list.AddRange( reference.Where(r => r.Item1 == columns[index]).Select( s => s.Item2) );
            }
            return baseList;
        }
    }
}
