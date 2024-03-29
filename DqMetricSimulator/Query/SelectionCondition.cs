﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Common.Linq;

namespace DqMetricSimulator.Query
{
    public partial class SelectionCondition : ISelectionCondition
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
            _expression = Evaluator.PartialEval(expression);
        }
    }
}