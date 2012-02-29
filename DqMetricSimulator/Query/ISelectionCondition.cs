using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DqMetricSimulator.Query
{
    public interface ISelectionCondition
    {
        Expression Expression { get; }
        HashSet<ParameterExpression> Parameters { get; }
        Delegate CompiledExpression { get; }
    }
}
