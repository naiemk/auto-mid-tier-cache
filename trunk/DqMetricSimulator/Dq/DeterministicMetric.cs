namespace DqMetricSimulator.Dq
{
    public class DeterministicMetric : IMetric
    {
        private readonly string _name;
        private readonly IMetricFunction _function;
        public IMetricFunction Function { get { return _function; } }

        public string Name { get { return _name; } }

        public DeterministicMetric(string name, IMetricFunction function)
        {
            _name = name;
            _function = function;
        }
    }
}
