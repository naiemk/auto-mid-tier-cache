using System.Collections.Generic;

namespace DqMetricSimulator.Core
{
    public class Row : IRow
    {
        private readonly IList<int> _rows = new List<int>();

        public IList<int> Rows
        {
            get { return _rows; }
        }
    }
}
