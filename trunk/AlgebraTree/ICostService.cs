using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DqMetricSimulator.Core;
using DqMetricSimulator.Query;

namespace AlgebraTree
{
    public interface ICostService
    {
        bool CanMaterialize(IQueryNode sample, IQuery query, ITable result);
    }

    public class NaiveCostService : ICostService
    {
        public bool CanMaterialize(IQueryNode sample, IQuery query, ITable result)
        {
            return true;
        }
    }
}
