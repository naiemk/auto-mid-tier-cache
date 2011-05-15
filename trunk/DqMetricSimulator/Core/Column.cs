using System.Collections.Generic;
using System.Linq;

namespace DqMetricSimulator.Core
{
    public class Column<T> : IColumn
    {
        private readonly List<T> _data = new List<T>();
        
        public T Get(int id)
        {
            return _data[id];
        }
        public object this[int id]
        {
            get { return _data[id]; }
            set { _data[id] = (T)value; }
        }
        public IList<T> Data
        {
            get { return _data; }
        }

        void IColumn.Add(object o)
        {
            Data.Add((T)o);
        }

        public Column()
        {}

        public Column(IEnumerable<T> data)
        {
            _data = data.ToList();
        }

        public int BinarySearch(T o)
        {
            return _data.BinarySearch(o);
        }

        int IColumn.BinarySearch(object o)
        {
            return BinarySearch((T) o);
        }

        public string Name { get; set; }


        public int Count
        {
            get { return _data.Count; }
        }
    }
}
