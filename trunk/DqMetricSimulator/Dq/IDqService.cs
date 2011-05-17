using DqMetricSimulator.Core;
using DqMetricSimulator.Query;

namespace DqMetricSimulator.Dq
{
    public interface IDqService
    {
        ITable UpdateMetricFunctions(IQuery query, ITable table);
    }

    public class SuperSimpleDqService : IDqService
    {
        public ITable UpdateMetricFunctions(IQuery query, ITable table)
        {
            return table;
        }
    }
}
