using System.Collections.Generic;

namespace DqMetricSimulator.Core
{
    public interface IRow
    {
        IList<int> Rows { get; }
    }
}