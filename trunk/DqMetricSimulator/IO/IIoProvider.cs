
using System;

namespace DqMetricSimulator.IO
{
    public interface IIoProvider
    {
        string[] ReadAllLines();
        void SetSource(string fileName);
    }

    public class FileIoProvider : IIoProvider
    {
        private string _source;
        public string[] ReadAllLines()
        {
            return System.IO.File.ReadAllLines(_source);
        }

        public void SetSource(string source)
        {
            _source = source;
            if (!System.IO.File.Exists(source))
            {
                throw new ArgumentException(String.Format("The file '{0}' does not exist.", source));
            }
        }
    }
}
