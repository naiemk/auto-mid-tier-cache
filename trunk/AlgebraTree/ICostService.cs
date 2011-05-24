using DqMetricSimulator.Core;
using DqMetricSimulator.Query;

namespace AlgebraTree
{
    public interface ICostService
    {
        bool CanMaterialize(IQueryNode sample, IQuery queryt);
    }

    public class NaiveCostService : ICostService
    {
        public long MemoryLimit { get; private set; }
        public long CallToDqServiceLimit { get; private set; }
        private int _queriesSoFar;
        private long _sizeSoFar;

        public bool CanMaterialize(IQueryNode sample, IQuery query)
        {
            _queriesSoFar += 1;
            _sizeSoFar += sample.Sample.Table.Rows.Count;
            return (_queriesSoFar < CallToDqServiceLimit && _sizeSoFar < MemoryLimit);
        }

        public NaiveCostService(long memoryLimit, long callToDqServiceLimit)
        {
            MemoryLimit = memoryLimit;
            CallToDqServiceLimit = callToDqServiceLimit;
        }
    }
}
