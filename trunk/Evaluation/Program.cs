using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evaluation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string connStr1 = "Data Source=.; Initial Catalog=AdventureWorksLT2008; Integrated Security=SSPI";
            const string connStr2 = "Data Source=.; Initial Catalog=DBLP; Integrated Security=SSPI";
            var toOut = new StringBuilder();
            //Mem limit is the limitation of memory
            var maxMem = 27;
            //Query limit is the limitation of call to Dq Service
            var maxQ = 60;
            var isFirst = true;
            for (maxMem = 30; maxMem < 1000; maxMem+=30)
            {
                //if (args[0] == "-naive-aw")
                //{
                //    DblpSamples.ExecuteTestForNaiveApproach(args[1], connStr1, DblpSamples.RangesForNaiveAw(), maxQ,
                //                                            maxMem, true);
                //}
                if (args[0] == "-naive-dblp")
                {
                    //Run with different max mem and maxQ and write back # of queries answered from sample per all queries.
                    toOut.Append(
                        DblpSamples.ExecuteTestForNaiveApproach(connStr2, DblpSamples.RangesForNaiveDblp(),
                                                                1.5f,
                                                                maxQ,
                                                                maxMem, isFirst)
                        );
                }
                if (args[0] == "-basic-dblp")
                {
                    //Run with different max mem and maxQ and write back # of queries answered from sample per all queries.
                    toOut.Append(
                        DblpSamples.RunTestBasic( DblpSamples.RangesForNaiveDblp(),
                                                                0.6f,
                                                                connStr2,
                                                                maxQ,
                                                                maxMem, isFirst)
                        );
                }
                isFirst = false;
            }
            System.IO.File.WriteAllText(args[1], toOut.ToString());
        }
    }
}
