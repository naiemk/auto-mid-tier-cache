using System.Collections.Generic;
using System.Linq;

namespace DqMetricSimulator.Query
{
    public interface IQuery
    {
        HashSet<IProjection> Projections { get; }
        HashSet<ISelectionCondition> SelectionConditions { get; }
        HashSet<string> Sources { get; }
    }

    public class BasicQuery:IQuery
    {
        private readonly HashSet<IProjection> _projections = new HashSet<IProjection>();

        private readonly HashSet<ISelectionCondition> _selectionConditions = new HashSet<ISelectionCondition>();

        private readonly HashSet<string> _sources = new HashSet<string>();

        public HashSet<IProjection> Projections
        {
            get { return _projections; }
        }

        public HashSet<ISelectionCondition> SelectionConditions
        {
            get { return _selectionConditions; }
        }

        public HashSet<string> Sources
        {
            get { return _sources; }
        }

        public BasicQuery()
        {}

        public BasicQuery(IEnumerable<IProjection> projections, IEnumerable<ISelectionCondition> selectionConditions, IEnumerable<string> sources)
        {
            projections.ToList().ForEach(p => _projections.Add(p));
            selectionConditions.ToList().ForEach(s => _selectionConditions.Add(s));
            sources.ToList().ForEach(s => _sources.Add(s));
        }
    }
}
