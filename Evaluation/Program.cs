using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DqMetricSimulator.Query;

namespace Evaluation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string connStr1 = "Data Source=.; Initial Catalog=AdventureWorksLT2008; Integrated Security=SSPI";
            //Mem limit is the limitation of memory
            //Run test for different queries, then put them together.

            var prg = new Prog();
            prg.RunEvaluationMultipleSampleRates();
        }

        
    }

}
