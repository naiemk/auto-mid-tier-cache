namespace DqMetricSimulator.Dq
{
    public interface IMetric
    {
        IMetricFunction Function { get; }
        string Name { get; }
    }
}
