using System.Collections.Generic;
using DqMetricSimulator.Core;
using DqMetricSimulator.Dq;

namespace DqMetricSimulator.Simulate
{
    /// <summary>
    /// Metric functions like Completenss, Consistency, Accuracy, etc are simulated using a lookup.
    /// Indeed, a random
    /// </summary>
    public class SimulatedMetrics : IMetricFunction
    {
        private readonly IDictionary<string, SimulatedMetricFunctionInfo> _lookup;
        private readonly string _name;

        public bool IsGood(IRow row, ITable table)
        {
            var keystring = table.GetKeystring(row);
            var inf = _lookup[keystring];
            System.Threading.Thread.Sleep(inf.Duration);
            return inf.Result;
        }


        public string Name { get { return _name; } }

        public SimulatedMetrics(string name, IDictionary<string , SimulatedMetricFunctionInfo> lookup)
        {
            _name = name;
            _lookup = lookup;
        }
    }
}
