using System;

namespace AlgebraTree
{
    public class SamplingOptions
    {
        private SamplingOptions()
        {
        }

        private static SamplingOptions _instance = null;
        private static Object _lockSingleton = new object();

        public static SamplingOptions Instance
        {
            get
            {
                lock (_lockSingleton)
                {
                    return _instance ?? (_instance = new SamplingOptions());
                }
            }
        }

        public bool LoockupIntersections
        {
            get { return false; }
        }
    }
}
