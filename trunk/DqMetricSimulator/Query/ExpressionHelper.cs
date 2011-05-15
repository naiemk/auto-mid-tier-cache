using System.Linq.Expressions;

namespace DqMetricSimulator.Query
{
    public static class ExpressionHelper
    {
        /// <summary>
        /// Metric function from expression
        /// </summary>
        public static IMetricFunction GetMetricFuntion(MethodCallExpression func)
        {
            if (func.Method.Name.StartsWith("m"))
                return null;
            return null;
        }
    }
}
