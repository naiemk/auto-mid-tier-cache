using System;
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
        private long _accumulativeQueries;
        private long _allumulativeSize;
        private double _queriesSoFar;
        public long MemoryLimit { get; set; }
        public long CallToDqServiceLimit { get; set; }
        public long MaxNoOfQueries { get { return _maxNoOfQueries; }}
        public long MaxSizeOfSamples { get { return _maxSizeOfSamples; } }
        private List<long> _sampleSizes = new List<long>();
        private List<double> _popularities = new List<double>();

        public BasicCostService(long callToDqServiceLimit, long memoryLimit, long maxNoOfQueries, long maxSizeOfSamples)
        {
            CallToDqServiceLimit = callToDqServiceLimit;
            MemoryLimit = memoryLimit;
            _maxNoOfQueries = maxNoOfQueries;
            _maxSizeOfSamples = maxSizeOfSamples;
        }

        public bool CanMaterialize(IQueryNode sample, IQuery query)
        {
            //Return if can materialize the sample based on its popularity considering
            //memory, and bandwith limitations.
            _queriesSoFar++;

            if (_accumulativeQueries >= MaxNoOfQueries || _allumulativeSize >= MemoryLimit)
                return false;

            var costs = sample.Sample.Table.Rows.Count;
            var benefit = sample.Popularity.Item1/_queriesSoFar;
            var tOfMem = (double)(MemoryLimit-_allumulativeSize)/(_maxSizeOfSamples-_allumulativeSize); //This says what rate of all queries can be materialized
            var tOfQuer = (double)(CallToDqServiceLimit - _accumulativeQueries) /(_maxNoOfQueries-_accumulativeQueries); //This says what rate of all queries can be materialized
            tOfQuer = Math.Max(tOfQuer, 1);
            var memJustifiable = _sampleSizes.Count > 1
                                     ? costs <=
                                       (_sampleSizes.Min() + tOfMem*(_sampleSizes.Max() - _sampleSizes.Min()))
                                     : costs <= tOfMem*MemoryLimit;
            var dqsJustifiable = _popularities.Count > 1
                                     ? sample.Popularity.Item1 >=
                                       (_popularities.Min() + tOfQuer*(_popularities.Max() - _popularities.Min()))
                                     : benefit >= _maxNoOfQueries*tOfQuer;
            _sampleSizes.Add(costs);
            _popularities.Add(sample.Popularity.Item1);
            _sampleSizes = GetLast100(_sampleSizes);
            _popularities = GetLast100(_popularities);
            var rv = memJustifiable && dqsJustifiable;
            if (rv)
            {
                _allumulativeSize += sample.Sample.Table.Rows.Count;
                _accumulativeQueries += sample.Popularity.Item1;
            }
            return rv;
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
