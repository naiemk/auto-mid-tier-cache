using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DqMetricSimulator.Query;

namespace AlgebraTree
{
    public interface  IQueryNode
    {
        IQueryNode Parent { get; }
        IQuery Query { get; }
        Popularity Popularity { get; }
        ISample Sample { get; }
        long Cardinality { get; set; }
        IList<IQueryNode> Childs { get; }
    }

    public class QueryNode : IQueryNode
    {
        private readonly IQueryNode _parent;

        private readonly IQuery _query;

        private readonly Popularity _popularity;

        private readonly ISample _sample;

        private readonly IList<IQueryNode> _childs = new List<IQueryNode>();

        public IQueryNode Parent
        {
            get { return _parent; }
        }

        public IQuery Query
        {
            get { return _query; }
        }

        public Popularity Popularity
        {
            get { return _popularity; }
        }

        public ISample Sample
        {
            get { return _sample; }
        }

        public long Cardinality { get; set; }

        public IList<IQueryNode> Childs { get { return _childs; } }

        public QueryNode(IQueryNode parent, IQuery query, Popularity popularity, ISample sample)
        {
            _parent = parent;
            _query = query;
            _popularity = popularity;
            _sample = sample;
        }
    }
}
