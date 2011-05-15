using System.Collections.Generic;

namespace DqMetricSimulator.Simulate
{
    public interface IHistogram<T> : IDictionary<T, float>
    {
    }

    public class Histogram<T> : Dictionary<T, float >, IHistogram<T>
    {
        
    }
}
