using System.Collections.Generic;
using System.Linq;
using DqMetricSimulator.Core;
using DqMetricSimulator.Query;

namespace AlgebraTree
{
    public class BasicCostService : ICostService
    {
        private readonly long _maxNoOfQueries;
        private readonly long _maxSizeOfSamples;
        private double _queriesSoFar;
        public long MemoryLimit { get; set; }
        public long CallToDqServiceLimit { get; set; }
        public long MaxNoOfQueries { get { return _maxNoOfQueries; }}
        public long MaxSizeOfSamples { get { return _maxSizeOfSamples; } }
        private List<long> _sampleSizes = new List<long>();
        private List<double> _popularities = new List<double>();

        public BasicCostService(long maxNoOfQueries, long maxSizeOfSamples)
        {
            _maxNoOfQueries = maxNoOfQueries;
            _maxSizeOfSamples = maxSizeOfSamples;
        }

        public bool CanMaterialize(IQueryNode sample, IQuery query, ITable result)
        {
            //Return if can materialize the sample based on its popularity considering
            //memory, and bandwith limitations.
            var costs = sample.Sample.Table.Rows.Count;
            var benefit = sample.Popularity.Item1/_queriesSoFar;
            _queriesSoFar++;
            var tOfMem = MemoryLimit/_maxSizeOfSamples; //This says what rate of all queries can be materialized
            var tOfQuer = CallToDqServiceLimit/_maxNoOfQueries; //This says what rate of all queries can be materialized
            var memJustifiable = costs <= (_sampleSizes.Min() + tOfMem*(_sampleSizes.Max() - _sampleSizes.Min()));
            var dqsJustifiable = benefit >= (_popularities.Min() + tOfQuer*(_popularities.Min() + _popularities.Max()));
            _sampleSizes.Add(costs);
            _popularities.Add(benefit);
            _sampleSizes = GetLast100(_sampleSizes);
            _popularities = GetLast100(_popularities);
            return dqsJustifiable && memJustifiable;
        }

        private static List<T> GetLast100<T>(List<T> input)
        {
            if (input.Count > 100)
            {
                input.Reverse();
                input = input.Take(100).ToList();
                input.Reverse();
            }
            return input;
        }
    }
}
