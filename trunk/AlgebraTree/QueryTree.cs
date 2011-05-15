
using System;

namespace AlgebraTree
{
    public class QueryTree
    {
        private readonly IQueryNode _root;
        public IQueryNode Root { get { return _root; } }

        public QueryTree(IQueryNode root)
        {
            _root = root;
        }

        public static QueryTree CreateEmptyTree()
        {
            var qn = new QueryNode(null, null, null, null);
            return new QueryTree(qn);
        }
    }
}
