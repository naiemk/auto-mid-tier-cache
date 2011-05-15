using System.Collections.Generic;

namespace DqMetricSimulator.Core
{
    public class Table : ITable
    {
        private readonly IMetadata _metadata;
        private readonly IList<IRow> _rows;
        private readonly IList<IColumn> _columns;

        public IMetadata Metadata
        {
            get { return _metadata; }
        }

        public IList<IRow> Rows
        {
            get { return _rows; }
        }

        public IList<IColumn> Columns
        {
            get { return _columns; }
        }

        public Table(IMetadata metadata, IList<IRow> rows, IList<IColumn> columns)
        {
            _metadata = metadata;
            _rows = rows;
            _columns = columns;
        }
    }
}
