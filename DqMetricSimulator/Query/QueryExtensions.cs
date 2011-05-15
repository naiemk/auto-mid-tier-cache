using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DqMetricSimulator.Query
{
    public static class QueryExtensions
    {
        public static bool IsSubsetOf(this ISelectionCondition me, ISelectionCondition other)
        {
            //x cond y
            //If equality -> x1 == x2 && y1 == y2
            //etc.
            var mex = me.Expression as BinaryExpression;
            var otherx = other.Expression as BinaryExpression;
            if (mex == null || otherx == null)
                return false;
            var x1 = mex.Left as ConstantExpression;
            var y1 = mex.Right as ConstantExpression;
            var x2 = otherx.Left as ConstantExpression;
            var y2 = otherx.Right as ConstantExpression;
            if ((x1 == null && y1 == null) || (x2 == null && y2 == null))
                return false; //At least one part of the expression should be constant.
            if ((x1 != null && y1 != null) || (x2 != null && y2 != null))
                return false; //At least one part of the expression should be non-constant.
            var var1 = x1 == null ? mex.Left.ToString() : mex.Right.ToString();
            var var2 = x2 == null ? otherx.Left.ToString() : otherx.Right.ToString();
            var con1 = x1 == null ? y1.Value : x1.Value;
            var con2 = x2 == null ? y2.Value : x2.Value;
            var op1 = x1 == null ? me.Expression.NodeType : Inverse(me.Expression.NodeType);
            var op2 = x2 == null ? other.Expression.NodeType : Inverse(other.Expression.NodeType);

            switch (op1)
            {
                case ExpressionType.Equal:
                    return var1 == var2 && con1 == con2;
                case ExpressionType.GreaterThanOrEqual:
                    return var1 == var2 &&
                           (op1 == op2) &&
                           con1 is IComparable &&
                           (((IComparable)con1).CompareTo(con2) >= 1);
                case ExpressionType.LessThanOrEqual:
                    return var1 == var2 &&
                           (op1 == op2) &&
                           con1 is IComparable &&
                           (((IComparable)con1).CompareTo(con2) <= 1);
                case ExpressionType.GreaterThan:
                    return var1 == var2 &&
                           (op1 == op2) &&
                           con1 is IComparable &&
                           (((IComparable)con1).CompareTo(con2) > 1);
                case ExpressionType.LessThan:
                    return var1 == var2 &&
                           (op1 == op2) &&
                           con1 is IComparable &&
                           (((IComparable)con1).CompareTo(con2) < 1);
                default: //Unsupported operation
                    return false;
            }
        }

        public static bool DoesIntersect(this ISelectionCondition me, ISelectionCondition other)
        {
            return IsSubsetOf(me, other);
        }

        private static ExpressionType Inverse(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Equal:
                    return ExpressionType.Equal;
                case ExpressionType.GreaterThanOrEqual:
                    return ExpressionType.LessThanOrEqual;
                case ExpressionType.LessThanOrEqual:
                    return ExpressionType.GreaterThanOrEqual;
                case ExpressionType.GreaterThan:
                    return ExpressionType.LessThan;
                case ExpressionType.LessThan:
                    return ExpressionType.GreaterThan;
                default:
                    return expressionType;
            }
        }

        /// <summary>
        /// Returns if "other" is subset of "me".
        /// </summary>
        public static bool IsSubsetOf(this IQuery me, IQuery other)
        {
            //All selection conditions of other should be superset of a selection condition of me
            return other.SelectionConditions.All(s => me.SelectionConditions.Any(mes => mes.IsSubsetOf(s)));
        }

        public static IEnumerable<int> GetKeyColumnsIds(this IQuery me)
        {
            return me.Projections.Select((p, i) => new {i, p}).Where(p => p.p.IsKey).Select(p => p.i);
        }

        /// <summary>
        /// Returns if "other" has intersection with "me".
        /// </summary>
        public static bool DoesIntersect(this IQuery me, IQuery other)
        {
            //Any selection conditions of other does intersect with of any selection condition of me
            //TODO: Fix this formula 
            return other.SelectionConditions.Any(s => me.SelectionConditions.Any(mes => mes.DoesIntersect(s)));
        }

        public static IMetricFunction GetMetricFunction(this IProjection me)
        {
            return (me.Expression.NodeType == ExpressionType.Call)
                       ? null
                       : ExpressionHelper.GetMetricFuntion(me.Expression as MethodCallExpression);
        }

    }
}
