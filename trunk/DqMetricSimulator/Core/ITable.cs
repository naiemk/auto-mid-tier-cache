using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DqMetricSimulator.Core
{
    public interface ITable
    {
        IMetadata Metadata { get; }
        IList<IRow> Rows { get; }
        IList<IColumn> Columns { get; }
    }

}
