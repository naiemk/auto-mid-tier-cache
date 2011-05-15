using System.Diagnostics;

namespace Common.Data.Sql
{
    public interface ILogger
    {
        void Log(string message);
    }
    public class DebugLogger : ILogger
    {
        public void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
