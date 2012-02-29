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
            Projections = new[]
                {
                    ProjectionItem.CreateFromName<Int32>("ProductId", true),
                    ProjectionItem.CreateFromName<String>("Name", false),
                    ProjectionItem.CreateFromName<String>("Color", false),
                    ProjectionItem.CreateFromBasicMetric("m_Completeness", "Name"),
                    ProjectionItem.CreateFromBasicMetric("m_Correctness", "Color")
                }.ToList();

        }

        public List<Tuple<String, ISelectionCondition>> Conds = new List<Tuple<String,ISelectionCondition>>();

        public List<IProjection> Projections { get; set; }
        public String Table { get; set; }

        public IQuery GetQuery(IEnumerable<ISelectionCondition> conds)
        {
            var rv = new BasicQuery(
                    Projections,
                conds.SelectMany(c => c is RangeSelection?new[]{ ((RangeSelection)c).R1, ((RangeSelection)c).R2 }:new[]
                                                                                                                      {
                                                                                                                          c
                                                                                                                      } ),
                new[] {Table}
                );
            return rv;
        }

        public QueryGenerator Range<T>(Expression<Func<T, bool>> e1, Expression<Func<T, bool>> e2)
        {
            var temp = new RangeSelection()
                           {
                               R1 = SelectionCondition.Lambda(e1),
                               R2 = SelectionCondition.Lambda(e2),
                           };
            Conds.Add(new Tuple<string, ISelectionCondition>(e1.Parameters[0].ToString(),
                                                             temp));
            return this;
        }

        public QueryGenerator Add<T>(Expression<Func<T, bool>> e)
        {
            Conds.Add(new Tuple<string, ISelectionCondition>(e.Parameters[0].ToString(),
                                                             SelectionCondition.Lambda(e)));
            return this;
        }

    }

    //This 
    internal class RangeSelection : ISelectionCondition
    {
        public Expression Expression
        {
            get { return null; }
        }

        public HashSet<ParameterExpression> Parameters
        {
            get { return null; }
        }

        public Delegate CompiledExpression
        {
            get { return null; }
        }

        public ISelectionCondition R1;
        public ISelectionCondition R2;
    }
}
