using System.Collections.Generic;
using DqMetricSimulator.Query;

namespace DqMetricSimulator.QueryFactory
{
    public abstract class QueryFactoryBase
    {
        public abstract IEnumerable<IQuery> Create(IDictionary<string, string> options);
    }
}
