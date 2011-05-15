
using System;

namespace DqMetricSimulator.Core
{
    public interface IColumn 
    {
        object this[int id] { get; set; }
        Int32 Count { get; }
        void Add(object o);
        int BinarySearch(object o);
        string Name { get; set; }
    }
}