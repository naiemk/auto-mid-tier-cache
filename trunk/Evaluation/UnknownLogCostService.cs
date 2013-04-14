using System;
using System.Collections.Generic;
using System.Linq;
using AlgebraTree;
using DqMetricSimulator.Query;

namespace Evaluation
{
    internal class UnknownLogCostService : ICostService
    {
        private int _maxCost = int.MinValue;
        private int _minCost = int.MaxValue;
        private int _minPop = int.MaxValue;
        private int _maxPop = int.MinValue;
        private readonly SortedList<double, IQueryNode> _distances = new SortedList<double, IQueryNode>();
        private readonly List<IQueryNode> _queries = new List<IQueryNode>();
        private readonly int _maxMovingAverageSize = 50;
        private readonly float _acceptRate;

        public UnknownLogCostService(float acceptRate, int movingAvgSize)
        {
            _acceptRate = acceptRate;
            _maxMovingAverageSize = movingAvgSize;
        }

        public bool CanMaterialize(IQueryNode sample, IQuery query)
        {
            var rv = false;
            _queries.Add(sample);
            _minCost = _queries.Min(q => q.Sample.Table.Rows.Count);
            _maxCost = _queries.Max(q => q.Sample.Table.Rows.Count);
            _minPop = _queries.Min(q => q.Popularity.Item1);
            _maxPop = _queries.Max(q => q.Popularity.Item1);
            if (_minCost == _maxCost || _minPop == _maxPop)
            {
                //only return true if the query is in the top C rate of sum of cost
                var sumCost = _queries.Sum(q => q.Sample.Table.Rows.Count);
                if ( sample.Sample.Table.Rows.Count < (_acceptRate * sumCost))
                rv = true;
            }
            else
            {
                var distance = GetDistance(sample.Cardinality, sample.Popularity);
                _distances.Add(distance, sample);
                var pivotPoint = _distances.ToArray()[_distances.Count - (int) (_distances.Count*_acceptRate) - 1].Key;
                rv = distance < pivotPoint;
            }
            if (_queries.Count > _maxMovingAverageSize)
            {
                var toDel = _queries[0];
                var toDelFromDistance = _distances.Select(d => new {d}).Where(d => d.d.Value.Equals(toDel)).FirstOrDefault();
                if (toDelFromDistance != null)
                    _distances.Remove(toDelFromDistance.d.Key);
                _queries.RemoveAt(0);
            }
            return rv;
        }

        private float GetDistance(long cost, Popularity pop)
        {
            return (float)Math.Pow(((float) (_maxCost - cost)/(_maxCost - _minCost)), 2)
                   + (float)Math.Pow(((float) (pop.Item1 - _minPop)/(_maxPop - _minPop)), 2);
        }
    }
}