using DqMetricSimulator.Core;
using DqMetricSimulator.Query;

namespace AlgebraTree
{
    public interface IDataService
    {
        ITable RunQuery(IQuery query);
        ITable RunQueryAgainstNode(IQuery query, IQueryNode node);
    }
}
