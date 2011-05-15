using DqMetricSimulator.Core;

namespace DqMetricSimulator.Dq
{
    public interface IMetricFunction
    {
        string Name { get; }
        bool IsGood(IRow row, ITable table);
    }
}
