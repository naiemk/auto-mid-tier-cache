using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DqMetricSimulator.Query;

namespace Evaluation
{
    public class Prog
    {

        public string Titles =
            "queries, totalTime, sampleCount, sampleSize, queryFromSample, confidenceThr, queryLimit, memLimit, samplingRate, answerRate, naive, basic, costModel, dqAware";

        private int _maxMem = 300;
        private float _confidence = 0.6f;
        private float _samplingRate = 0.05f;
        private long _maxQ = 600;
        private string _connStr = "Data Source=.; Initial Catalog=DBLP; Integrated Security=SSPI";
        private List<List<double>> _result = new List<List<double>>();
        private IEnumerable<IQuery> _queries;
        private int _maxSizeOfSamples;
        private Action<List<List<double>>>  _onWriteBack = null;

        private void RunNaive()
        {
            var csr = _confidence;
            _confidence = 1.5f;
            RunBasic();
            _confidence = csr;
        }

        private  void RunCostBased()
        {
                        //Run with different max mem and maxQ and write back # of queries answered from sample per all queries.
                        _result.Add(
                            DblpSamples.RunTestBasic(_queries,
                                                     _confidence,
                                                     _connStr,
                                                     _maxQ,
                                                     _maxMem,
                                                     _samplingRate,
                                                     _maxSizeOfSamples
                                                     ).ToList()
                            );
        }
        private  void RunBasic()
        {
                        //Run with different max mem and maxQ and write back # of queries answered from sample per all queries.
                        _result.Add(
                            DblpSamples.RunTestNaive(_queries,
                                                     _confidence,
                                                     _connStr,
                                                     _maxQ,
                                                     _maxMem,
                                                     _samplingRate
                                                     ).ToList()
                            );
        }
        private void RunDq()
        {
            var ct = _confidence;
            _confidence = 0.0f;
            RunCostBased();
            _confidence = ct;
        }

        public static void WriteResult(string fileName, string ttl, IEnumerable<IEnumerable<double>> data)
        {
            var lines = data.Select(GetLine).ToList();
            if(ttl != null)
                lines.Insert(0, ttl);
            System.IO.File.WriteAllLines(fileName, lines.ToArray());
        }
        public static string GetLine(IEnumerable<double > data)
        {
            var sb = new StringBuilder();
            data.Select((d, i) => new {i,d}).ToList().ForEach((d => sb.Append( (d.i==0?"":",") + d.d.ToString())));
            return sb.ToString();
        }

        public void RunEvaluationMultipleSampleRates()
        {
            _queries = DblpSamples.RangesForNaiveDblp();

            var basics = new List<List<double>>();
            var costbases = new List<List<double>>();

            _onWriteBack = o =>
                               {
                                   basics.Add(o.Select(l => l[11]).ToList());
                                   costbases.Add(o.Select(l => l[12]).ToList());
                               };

            for (_samplingRate = 0.01f; _samplingRate < 0.6f; _samplingRate+=0.05f)
            {
                _maxSizeOfSamples = (int) (700.0*(_samplingRate/0.05f));
                RunEvaluationMultipleMem((int) (_samplingRate*10));
            }

            WriteResult("C:\\samplesDiff_basic.csv", null, basics);
            WriteResult("C:\\samplesDiff_costBased.csv", null, costbases);

        }

        public void RunEvaluationDifferentLevels()
        {
            _samplingRate = 0.1f;
            _queries = DblpSamples.RangesForDblpMidCoverage1(500, 40, 0, 0, 0, 0);
            _maxSizeOfSamples = (int)(_samplingRate * ( 40*500  ));
            RunEvaluationMultipleMem(1);
            _queries = DblpSamples.RangesForDblpMidCoverage1(500, 20, 4000, 05, 000, 0);
            _maxSizeOfSamples = (int)(_samplingRate * ( 20*500 + 05*4000 ));
            RunEvaluationMultipleMem(2);
            _queries = DblpSamples.RangesForDblpMidCoverage1(500, 20, 4000, 06, 6000, 1);
            _maxSizeOfSamples = (int)(_samplingRate * ( 20*500 + 06*4000 + 1*6000 ));
            RunEvaluationMultipleMem(3);
            _queries = DblpSamples.RangesForDblpMidCoverage1(500, 20, 2000, 10, 6000, 1);
            _maxSizeOfSamples = (int)(_samplingRate * ( 20*500 + 10*2000 + 1*6000 ));
            RunEvaluationMultipleMem(4);
            _queries = DblpSamples.RangesForDblpMidCoverage1(500, 20, 2000, 10, 6000, 3);
            _maxSizeOfSamples = (int)(_samplingRate * ( 20*500 + 20*2000 + 10*6000 ));
            RunEvaluationMultipleMem(5);
            _queries = DblpSamples.RangesForDblpMidCoverage1(500, 20, 1000, 10, 2000, 5);
            _maxSizeOfSamples = (int)(_samplingRate * ( 20*500 + 20*1000 + 9*2000 ));
            RunEvaluationMultipleMem(6);
        }

        public void RunEvaluationMultipleMem(int num)
        {
            //Should run the eval.
            var minMem = 100;
            var maxMem = 2500;
            var memDif = 200;
            Enumerable.Range(0, (maxMem - minMem) / memDif).ToList()
                .ForEach(i => { _maxMem = minMem + i * memDif; RunNaive(); });
            var res1 = _result;
            _result = new List<List<double>>();
            Enumerable.Range(0, (maxMem - minMem) / memDif).ToList()
                .ForEach(i => { _maxMem = minMem + i * memDif; RunBasic(); });
            var res2 = _result;
            _result = new List<List<double>>();
            Enumerable.Range(0, (maxMem-minMem)/memDif).ToList()
                .ForEach(i => { _maxMem = minMem + i * memDif; RunCostBased(); });
            var res3 = _result;
            _result = new List<List<double>>();
            Enumerable.Range(0, (maxMem-minMem)/memDif).ToList()
                .ForEach(i => { _maxMem = minMem + i * memDif; RunDq(); });
            var res4 = _result;
            _result = new List<List<double>>();
            var vals = MergeResults(d => (d[4] + d[2]) / d[0], res1, res2, res3, res4);
            if (_onWriteBack == null)
                WriteResult("C:\\outMerged" + num + ".csv", Titles, vals);
            else
                _onWriteBack(vals);
        }
        public void RunEvaluationMultipleMem()
        {
            //Should run the eval.
            _queries = DblpSamples.RangesForNaiveDblp();
            var minMem = 30;
            var maxMem = 900;
            var memDif = 30;
            Enumerable.Range(0, (maxMem-minMem)/memDif).ToList()
                .ForEach(i => { _maxMem = minMem + i * memDif; RunNaive(); });
            var res1 = _result;
            _result = new List<List<double>>();
            Enumerable.Range(0, (maxMem-minMem)/memDif).ToList()
                .ForEach(i => { _maxMem = minMem + i * memDif; RunBasic(); });
            var res2 = _result;
            _result = new List<List<double>>();
            Enumerable.Range(0, (maxMem-minMem)/memDif).ToList()
                .ForEach(i => { _maxMem = minMem + i * memDif; RunCostBased(); });
            var res3 = _result;
            _result = new List<List<double>>();
            Enumerable.Range(0, (maxMem-minMem)/memDif).ToList()
                .ForEach(i => { _maxMem = minMem + i * memDif; RunDq(); });
            var res4 = _result;
            _result = new List<List<double>>();
            var vals = MergeResults(d => (d[4] + d[2])/d[0], res1, res2, res3, res4);
            if (_onWriteBack == null)
                WriteResult("C:\\outMerged.csv", Titles, vals);
            else
                _onWriteBack(vals);
        }

        public List<List<double>> MergeResults(Func<double[], double> valSelector, params List<List<double>>[] ins)
        {
            return ins[0].Select((fl, i) => ins[0][i].Concat(ins.Select(sl => valSelector(sl[i].ToArray()))).ToList()).
            ToList();
        }
        
    }
}