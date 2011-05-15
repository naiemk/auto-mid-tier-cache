using System;

namespace Common.Data.Sql
{
    public interface IProgress
    {
        void Progressed(int i);
        void Reset(int maximulEstimate);
        void Finish();
    }

    public class Progress : IProgress
    {
        private readonly ILogger _logger;
        private readonly int _stepDist;
        private double _progress;
        private int _lastPrint;
        private int _max = 100;
        public void Progressed(int i)
        {
            _progress += 100.0 * i / _max;
            if (((int) _progress) < (_lastPrint + _stepDist)) return;
            _logger.Log(String.Format("{0} %", (int)_progress));
            _lastPrint = (int)_progress;
        }

        public void Reset(int maximulEstimate)
        {
            _progress = 0;
            _lastPrint = 0;
            _max = maximulEstimate;
        }

        public void Finish()
        {
            _progress = 100;
            _lastPrint = 100;
            _logger.Log(String.Format("{0} %", (int)_progress));
        }

        public Progress(ILogger logger, int stepDist)
        {
            _logger = logger;
            _stepDist = stepDist;
        }
    }
}
