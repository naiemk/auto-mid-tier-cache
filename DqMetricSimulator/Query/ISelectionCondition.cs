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

    public class SelectionCondition : ISelectionCondition
    {
        private readonly Expression _expression;

        private readonly HashSet<ParameterExpression> _parameters;

        private Delegate _compiledExpression;

        public Expression Expression
        {
            get { return _expression; }
        }

        public Delegate CompiledExpression
        {
            get
            {
                if (_compiledExpression == null)
                       _compiledExpression = Expression.Lambda(_expression, false, _parameters).Compile();
                return _compiledExpression;
            }
        }

        public HashSet<ParameterExpression> Parameters
        {
            get { return _parameters; }
        }

        public SelectionCondition(HashSet<ParameterExpression> parameters, Expression expression)
        {
            _parameters = parameters;
            _expression = expression;
        }

        public static ISelectionCondition CreateFromLambda<T>(Expression<Func<T, bool>> e)
        {
            return new SelectionCondition(
                    new HashSet<ParameterExpression>(e.Parameters),
                    e.Body
                );
        }
    }
}
