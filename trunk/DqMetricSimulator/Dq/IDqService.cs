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

        /// <summary>
        /// These are just proxy classes to allow definition of metric function in query. The actual method would exists on the service.
        /// </summary>
        public static bool m_Completeness(string param)
        {
            return true;
        }

        public static bool m_Correctness(string param)
        {
            return true;
        }
    }
}
