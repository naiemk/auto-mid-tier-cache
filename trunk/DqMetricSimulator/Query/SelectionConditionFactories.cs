using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DqMetricSimulator.Query
{
    public partial class SelectionCondition
    {
        public static ISelectionCondition Lambda<T>(Expression<Func<T, bool>> e)
        {
            return new SelectionCondition(
                new HashSet<ParameterExpression>(e.Parameters),
                e.Body
                );
        }

        public static ISelectionCondition Equality<T>(string attributeName, T value)
        {
            var param = Expression.Parameter(typeof (T), attributeName);
            var parameters = new HashSet<ParameterExpression>();
            var valExp = Expression.Constant(value, typeof (T));
            var expression = Expression.Equal(param, valExp);
            return new EqualitySelectionCondition(parameters, expression);
        }
    }
}
