using System.Collections.Generic;

namespace DqMetricSimulator.Statistics
{
    public static class Utils
    {
        public static IEnumerable<int> Sequence(int from, int to)
        {
            for (var i = from; i < to; i++)
                yield return i;
        }
    }
}
