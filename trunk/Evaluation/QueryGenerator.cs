using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DqMetricSimulator.Query;

namespace Evaluation
{
    /// <summary>
    /// This class keeps a template and generates queries with different parameters.
    /// </summary>
    public class QueryGenerator
    {
        public void Initialize()
        {
            //Set the template for each query.

        }

        public List<Tuple<String, ISelectionCondition>> Conds = new List<Tuple<String,ISelectionCondition>>();

        public static IQuery GetQuery(IEnumerable<ISelectionCondition> conds)
        {
            var rv = new BasicQuery(
                new[]
                    {
                        ProjectionItem.CreateFromName<Int32>("ProductId", true),
                        ProjectionItem.CreateFromName<String>("Name", false),
                        ProjectionItem.CreateFromName<String>("Color", false),
                        ProjectionItem.CreateFromMetric("m_Completeness", "Name"),
                        ProjectionItem.CreateFromMetric("m_Correctness", "Color")
                    },
                conds,
                new[] {"SaleLT.Product"}
                );
            return rv;
        }

        public QueryGenerator Range<T>(Expression<Func<T, bool>> e1, Expression<Func<T, bool>> e2)
        {
            Conds.Add(new Tuple<string, ISelectionCondition>(e1.Parameters[0].ToString(),
                                                             SelectionCondition.CreateFromLambda(e1)));
            Conds.Add(new Tuple<string, ISelectionCondition>(e1.Parameters[0].ToString(),
                                                             SelectionCondition.CreateFromLambda(e2)));
            return this;
        }

        public QueryGenerator Add<T>(Expression<Func<T, bool>> e)
        {
            Conds.Add(new Tuple<string, ISelectionCondition>(e.Parameters[0].ToString(),
                                                             SelectionCondition.CreateFromLambda(e)));
            return this;
        }

    }
}
