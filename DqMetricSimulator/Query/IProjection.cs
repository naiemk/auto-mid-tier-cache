using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DqMetricSimulator.Query
{
    public interface IProjection
    {
        HashSet<string> Columns { get; }
        Expression Expression { get; }
        bool IsKey { get; }
    }

    public class ProjectionItem : IProjection
    {
        private readonly HashSet<string> _columns;
        private readonly Expression _expression;
        private readonly bool _isKey;

        public HashSet<string> Columns { get { return _columns; } }
        public Expression Expression { get { return _expression; } }
        public bool IsKey{get { return _isKey; }}

        public ProjectionItem(HashSet<string> columns, Expression expression, bool isKey)
        {
            _columns = columns;
            _expression = expression;
            _isKey = isKey;
        }

        public static IProjection CreateFromName<TIn>(string colName, bool isKey)
        {
            return new ProjectionItem(
                new HashSet<string>(new[] {colName}),
                (Expression<Func<TIn, TIn>>)(c => c),
                isKey
                );
        }
    }
}
