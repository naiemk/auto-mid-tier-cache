using System.Collections.Generic;
using System.Linq;

namespace DqMetricSimulator.Core
{
    public class Metadata : IMetadata
    {
        readonly List<int> _keyCols = new List<int>();

        public IList<int> KeyCols
        {
            get { return _keyCols; }
        }

        public IEnumerable<object> GetKeyValuesForRow(ITable table, IRow row)
        {
            return _keyCols.Select(keyCol => table.Columns[keyCol][row.Rows[keyCol]]);
        }
    }
}
