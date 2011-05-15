using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AlgebraTree;
using Common;
using Common.Data.Sql;
using DqMetricSimulator.Core;
using DqMetricSimulator.Data;
using DqMetricSimulator.Query;

namespace AlgebraToSqlServer
{
    public class SimpleSqlDataService : IDataService 
    {
        public Dal Dal { get; private set; }
        private const string ConnStr = "";
        /// <summary>
        /// Runs a query aggainst database
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ITable RunQuery(IQuery query)
        {
            var sb = new StringBuilder();
            sb.Append(" select ");
            //add columns
            sb.Append(query.Projections.SelectMany(p => p.Columns).Select(c => "[" + c + "]").JoinStrings(", "));
            sb.Append(" from ");
            sb.Append(query.Sources.Select(c => "[" + c + "]").JoinStrings(", "));
            if (query.SelectionConditions.Count > 0)
            {
                sb.Append(" where ");
                sb.Append(query.SelectionConditions.Select(SelectionConditionToString).JoinStrings(" AND "));
            }
            var dt  = Dal.GetDataTable(ConnStr, sb.ToString());
            return ImportDataTable.ImportAdoNetDataTable(dt);
        }

        /// <summary>
        /// Currently, we only support equality and range conditions.
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        private static string SelectionConditionToString(ISelectionCondition sc)
        {
            var sb = new StringBuilder();
            //Different possibilities:
            //1-Single parameter => Comparision with a constant
            //2-Double parameter => Join 
            var exp = sc.Expression;
            if (sc.Parameters.Count == 1)
            {   //Should comparision with a constant
                //We support equality, greaterthan and lessthan
                Expect<BinaryExpression>(exp);
                var eqex = (BinaryExpression) exp;
                var firstPar = sc.Parameters.First();
                switch (exp.NodeType)
                {
                    case ExpressionType.Equal :
                        sb.AppendFormat("{0} = {1}", firstPar, eqex.Left is ParameterExpression ? eqex.Right : eqex.Left);
                        break;
                    case ExpressionType.GreaterThanOrEqual :
                        sb.AppendFormat("{0} <= {1}", firstPar, eqex.Left is ParameterExpression ? eqex.Right : eqex.Left);
                        break;
                    case ExpressionType.LessThanOrEqual :
                        sb.AppendFormat("{0} >= {1}", firstPar, eqex.Left is ParameterExpression ? eqex.Right : eqex.Left);
                        break;
                    default:
                        throw new NotSupportedException(String.Format("Expression '{0}' not supported.", exp));
                }
            }
            else
                if (sc.Parameters.Count == 2)
            {   //Should be a join statement, only equality supported
                Expect<BinaryExpression>(exp);
                var binPer = (BinaryExpression) exp;
                if (binPer.NodeType == ExpressionType.Equal)
                {
                    var par1 = sc.Parameters.ToList()[0];
                    var par2 = sc.Parameters.ToList()[1];
                    sb.AppendFormat("{0}={1}", par1, par2);
                }
                else
                {
                    throw new NotSupportedException(String.Format(" {0} not supported.", binPer ));
                }
            }
                else
                {
                    throw new NotSupportedException(String.Format("Expression {0} has more than 2 parameters which is not supported", sc.Expression));
                }
            return sb.ToString();
        }

        private static void Expect<T>(Expression exp) where T : class
        {
            var newEx = exp as T;
            if (newEx == null)
                throw new SyntaxErrorException("Error at" + exp);
        }

        public ITable RunQueryAgainstNode(IQuery query, IQueryNode node)
        {
            //Steps:
            //From node.sample, where query.condition, select query.projection
            //
            //Query.condition can be assessed with passing 
            var rv = node.Sample.Table.Rows.Where(
                    //Query condition
                    r => query.SelectionConditions.All(c => (bool)c.CompiledExpression.Method.Invoke(c, 
                            //Extract parameter values
                            c.Parameters.Select(p => node.Sample.Table.GetValueByColumn(p.Name, r)).ToArray())
                            )
                        ).Select(r => r);
            var tbl = TableFactory.CreateTable(query.SelectionConditions.Count, query.GetKeyColumnsIds());
            //TODO:Fill in the table
            return tbl;
        }

        public SimpleSqlDataService()
        {
            var logger = new DebugLogger();
            Dal = new Dal(logger, new Progress(logger, 10));
        }
    }
}
