using DqMetricSimulator.Core;

namespace AlgebraTree
{
    public class EstimationResult
    {
        private readonly ITable _result;
        private readonly float _confidence;

        public ITable Result { get { return _result; } }
        public float Confidence { get { return _confidence; } }

        public EstimationResult(ITable result, float confidence)
        {
            _result = result;
            _confidence = confidence;
        }
    }
}
