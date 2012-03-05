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
            _queries.Add(sample);
            _minCost = _queries.Min(q => q.Sample.Table.Rows.Count);
            _maxCost = _queries.Max(q => q.Sample.Table.Rows.Count);
            _minPop = _queries.Min(q => q.Popularity.Item1);
            _maxPop = _queries.Max(q => q.Popularity.Item1);
            if (_minCost == _maxCost || _minPop == _maxPop)
                return true;
            var distance = GetDistance(sample.Cardinality, sample.Popularity);
            _distances.Add(distance, sample);
            var pivotPoint = _distances.ToArray()[_distances.Count - (int) (_distances.Count*_acceptRate) - 1].Key;
            if (_queries.Count > _maxMovingAverageSize)
            {
                var toDel = _queries[0];
                _distances.Remove(_distances.Where(d => d.Value.Equals(toDel)).First().Key);
                _queries.RemoveAt(0);
            }
            return distance < pivotPoint;
        }

        private float GetDistance(long cost, Popularity pop)
        {
            return (float)Math.Pow(((float) (_maxCost - cost)/(_maxCost - _minCost)), 2)
                   + (float)Math.Pow(((float) (pop.Item1 - _minPop)/(_maxPop - _minPop)), 2);
        }
    }
}