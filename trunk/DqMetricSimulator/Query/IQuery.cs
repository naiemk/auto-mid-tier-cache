using System.Collections.Generic;

namespace DqMetricSimulator.Query
{
    public interface IQuery
    {
        HashSet<IProjection> Projections { get; }
        HashSet<ISelectionCondition> SelectionConditions { get; }
        HashSet<string> Sources { get; }
    }
}
