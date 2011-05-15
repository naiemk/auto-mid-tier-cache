using System;
using System.Collections.Generic;
using System.Linq;
using DqMetricSimulator.Core;

namespace DqMetricSimulator.Simulate
{
    public static class SimulationBuilder
    {
        public static IHistogram<Int32> GetNormalDistributedBaseHistogram()
        {
            var dist = new Statistics.NormalDist(0, 1);
            return GetRandomBaseHistogram(100, i => (float)(1.0 - 2.0 * dist.PDF((i-50.0)/12.5)));
        }

        public static IHistogram<Int32> GetRandomBaseHistogram(int size, Func<Int32, float> randomFunction)
        {
            var rv = new Histogram<Int32>();
            //Gets a random base histogram
            for(int i =0; i< size; i++)
                rv.Add(i, randomFunction(i));
            return rv;
        }

        public static IHistogram<T> GetHistogram<T>(Column<T> column, IHistogram<Int32> baseHistogram)
        {
            if (baseHistogram.Count == 0)
                throw new DivideByZeroException("Base Histogram CANNOT be Empty!");
            var ratio = (double)column.Data.Count/baseHistogram.Values.Count;
            var rv = new Histogram<T>();
            var lastIdx = 0;
            for(var i = 0; i < baseHistogram.Count; i++)
            { 
                var newIdx = (int) (i * ratio);
                for(var j =lastIdx; j<=newIdx; j++)
                {
                    rv.Add(column.Get(j), baseHistogram[i]);
                    lastIdx = j+1;
                }
            }
            return rv;
        }

        public static IDictionary<string, SimulatedMetricFunctionInfo> CreateSimulatedLoockup<T>(ITable sourceTable, int targetColumn, IHistogram<T> histogram, Func<IRow, TimeSpan> durationFunc)
        {
            return CreateSimulatedLoockup(sourceTable, sourceTable.Rows, targetColumn, histogram, durationFunc);
        }
        /// <summary>
        /// Creates a dictionary of simulated results for a metric function.
        /// Returns a dictionary of keys=>result
        /// </summary>
        public static IDictionary<string, SimulatedMetricFunctionInfo> CreateSimulatedLoockup<T>(ITable sourceTable, IList<IRow> rows, int targetColumn, IHistogram<T> histogram, Func<IRow, TimeSpan> durationFunc)
        {
            var sourceCol = sourceTable.Columns[targetColumn];

            return rows.GroupBy(r => r.Rows[targetColumn])
                .Where(g => histogram.ContainsKey((T) sourceCol[g.Key]))
                .SelectMany(g => GetSimulatedMetricFunctionInfo(sourceTable, g.ToList(), histogram[(T) sourceCol[g.Key]], durationFunc))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static IEnumerable<KeyValuePair<string, SimulatedMetricFunctionInfo>> GetSimulatedMetricFunctionInfo(ITable sourceTable, List<IRow> list, float percentGood, Func<IRow, TimeSpan> durationFunc)
        {
            var goodThr = (int) (percentGood*list.Count);
            return list.Select((r, i) =>
                        new KeyValuePair<string, SimulatedMetricFunctionInfo>
                            (
                            sourceTable.GetKeystring(r),
                            new SimulatedMetricFunctionInfo {Duration = durationFunc(r), Result = i <= goodThr}
                            )
                );
        }
    }
}
