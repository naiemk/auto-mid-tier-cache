using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DqMetricSimulator.Dq;

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
                    return (var1 == var2 && ConstantIsEqual(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThan && ConstantIsGt(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThanOrEqual && ConstantIsGte(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.LessThan && ConstantIsGt(con2, con1)) ||
                           (var1 == var2 && op2 == ExpressionType.LessThanOrEqual && ConstantIsGt(con2, con1));
                case ExpressionType.GreaterThanOrEqual:
                    return //(var1 == var2 && op2 == ExpressionType.Equal && ConstantIsGte(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThan && ConstantIsGt(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThanOrEqual && ConstantIsGte(con1, con2));
                case ExpressionType.LessThanOrEqual:
                    return //(var1 == var2 && op2 == ExpressionType.Equal && ConstantIsGte(con2, con1)) ||
                           (var1 == var2 && op2 == ExpressionType.LessThan && ConstantIsGt(con2, con1)) ||
                           (var1 == var2 && op2 == ExpressionType.LessThanOrEqual && ConstantIsGte(con2, con1));
                case ExpressionType.GreaterThan:
                    return //(var1 == var2 && op2 == ExpressionType.Equal && ConstantIsGt(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThan && ConstantIsGte(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThanOrEqual && ConstantIsGte(con1, con2));
                case ExpressionType.LessThan:
                    return //(var1 == var2 && op2 == ExpressionType.Equal && ConstantIsGt(con2, con1)) ||
                           (var1 == var2 && op2 == ExpressionType.LessThan && ConstantIsGte(con2, con1)) ||
                           (var1 == var2 && op2 == ExpressionType.LessThanOrEqual && ConstantIsGte(con2, con1));
                default: //Unsupported operation
                    return false;
            }
        }

        public static bool DoesIntersect(this ISelectionCondition me, ISelectionCondition other)
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
                    return (var1 == var2 && ConstantIsEqual(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThan && ConstantIsGt(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThanOrEqual && ConstantIsGte(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.LessThan && ConstantIsGt(con2, con1)) ||
                           (var1 == var2 && op2 == ExpressionType.LessThanOrEqual && ConstantIsGt(con2, con1));
                case ExpressionType.GreaterThanOrEqual:
                    return (var1 == var2 && op2 == ExpressionType.Equal && ConstantIsGte(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThan) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThanOrEqual) ||
                           (var1 == var2 && op2 == ExpressionType.LessThan && ConstantIsGte(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.LessThanOrEqual && ConstantIsGte(con1, con2));
                case ExpressionType.GreaterThan:
                    return (var1 == var2 && op2 == ExpressionType.Equal && ConstantIsGt(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThan) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThanOrEqual) ||
                           (var1 == var2 && op2 == ExpressionType.LessThan && ConstantIsGt(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.LessThanOrEqual && ConstantIsGte(con1, con1));
                case ExpressionType.LessThanOrEqual:
                    return (var1 == var2 && op2 == ExpressionType.Equal && ConstantIsGte(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThan && ConstantIsGt(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThanOrEqual && ConstantIsGte(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.LessThan) ||
                           (var1 == var2 && op2 == ExpressionType.LessThanOrEqual);
                case ExpressionType.LessThan:
                    return (var1 == var2 && op2 == ExpressionType.Equal && ConstantIsGte(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThan && ConstantIsGt(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.GreaterThanOrEqual && ConstantIsGte(con1, con2)) ||
                           (var1 == var2 && op2 == ExpressionType.LessThan) ||
                           (var1 == var2 && op2 == ExpressionType.LessThanOrEqual);
                default: //Unsupported operation
                    return false;
            }
        }

        private static bool ConstantIsEqual(object con1, object con2)
        {
            return con1.Equals(con2);
        }


        /// <summary>
        /// Returns if con1 is greated than or equal to con2
        /// </summary>
        private static bool ConstantIsGte(object con1, object con2)
        {
            return con1 is IComparable && ((IComparable) con1).CompareTo(con2) >= 0;
        }

        /// <summary>
        /// Returns if con1 is greated than to con2
        /// </summary>
        private static bool ConstantIsGt(object con1, object con2)
        {
            return con1 is IComparable && ((IComparable) con1).CompareTo(con2) > 0;
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
            //For any groupby  selection paremeter, all selection conditions of other does intersect with of any selection condition of me
            var meCondsByPara = me.SelectionConditions.GroupBy(s => s.Parameters.FirstOrDefault().ToString()).ToList();
            var otherCondsByPara = other.SelectionConditions.GroupBy(s => s.Parameters.FirstOrDefault().ToString()).ToList();

            //Queries intersect if for all common groups, all conditions intersec
            return meCondsByPara.Join(otherCondsByPara, g => g.Key, o => o.Key, (m, o) => new {m, o})
                .All(j => j.m.All(s1 => j.o.Any(s2 => s1.DoesIntersect(s2))));
        }

        public static IMetricFunction GetMetricFunction(this IProjection me)
        {
            return (me.Expression.NodeType == ExpressionType.Call)
                       ? null
                       : ExpressionHelper.GetMetricFuntion(me.Expression as MethodCallExpression);
        }

    }
}
