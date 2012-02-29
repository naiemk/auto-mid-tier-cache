using System.Collections.Generic;
using System.Linq.Expressions;

namespace DqMetricSimulator.Query
{
    public class EqualitySelectionCondition : SelectionCondition
    {
        public EqualitySelectionCondition(HashSet<ParameterExpression> parameters, Expression expression) : base(parameters, expression)
        {
        }
    }
}
