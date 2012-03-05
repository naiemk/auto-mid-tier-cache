using System;
using System.Collections.Generic;
using System.Linq;
using AlgebraTree;
using DqMetricSimulator.Query;

namespace Evaluation
{
    public class PreKnownLogCostService : ICostService
    {
        private readonly List<IQuery> _reasonableQueries;
        public PreKnownLogCostService(int size, IEnumerable<PreKnownQuery> queries) //TODO: Get the popularity of each query, get the cost of each query
        {
            var qPops = queries.Select(q => new { q, pop = queries.Sum(q2 => q2.Query.IsSubsetOf(q.Query) ? 1 : 0) }
                ).ToList();
            var minPop = qPops.Min(qp => qp.pop);
            var maxPop = qPops.Max(qp => qp.pop);
            var minCost = qPops.Min(qp => qp.q.Cost);
            var maxCost = qPops.Max(qp => qp.q.Cost);
            var sortedQueries = qPops.OrderBy(qp => CostFormula(qp.q, qp.pop, minCost, maxCost, minPop, maxPop));
            var accumCost = default(long);
            _reasonableQueries = sortedQueries.TakeWhile(qp =>
                                                             {
                                                                 accumCost += qp.q.Cost;
                                                                 return accumCost <= size;
                                                             }
                ).Select(qp => qp.q.Query).ToList();
        }

        private static double CostFormula(PreKnownQuery preKnownQuery, int pop, int minCost, int maxCost, int minPop, int maxPop)
        {
            return (Math.Pow(1.0 * (preKnownQuery.Cost - minCost) / (maxCost - minCost), 2) +
                    Math.Pow(1.0 * (maxPop - pop) / (maxPop - minPop), 2));
        }

        public bool CanMaterialize(IQueryNode sample, IQuery query)
        {
            var rv = _reasonableQueries.Any(q => q.IsSubsetOf(query) && query.IsSubsetOf(q));
            return rv;
        }
    }

    public class PreKnownQuery
    {
        public int Popularity { get; set; }
        public int Cost { get; set; }
        public IQuery Query { get; set; }
    }
}