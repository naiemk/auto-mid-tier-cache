using DqMetricSimulator.Core;

namespace AlgebraTree
{
    public interface ISample
    {
        ITable Table { get; }
        bool Materialized { get; }
        void Materialize();
    }

    public class Sample : ISample
    {
        private readonly ITable _table;

        public ITable Table
        {
            get { return _table; }
        }

        public bool Materialized { get; private set; }
        public void Materialize()
        {
            Materialized = true;
        }

        public Sample(ITable table, bool materialized)
        {
            _table = table;
            Materialized = materialized;
        }
    }
}
