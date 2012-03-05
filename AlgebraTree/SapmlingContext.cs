using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using DqMetricSimulator.Core;
using DqMetricSimulator.Data;
using DqMetricSimulator.Dq;
using DqMetricSimulator.Query;

namespace AlgebraTree
{
    public class SapmlingContext
    {
        private readonly IQueryAnsweringService _queryAnsweringService;
        private readonly ICostService _costService;
        private readonly IDataService _dataServicea;
        private readonly IDqService _dqService;
        public QueryTree Tree { get; private set; }
        public float ConfidenceThreshold { get; set; }
        public float SamplingRate { get; set; }
        public Action<IQuery> PreQueryRunEvent { get; set; }
        public Action<IQuery> PostQueryRunEvent = (q) => { };
        public Action<IQuery, ISample> SampleMaterialzied = (q, s) => { };
        
        public void Initialize()
        {
            Tree = QueryTree.CreateEmptyTree();
        }

        public SapmlingContext(IQueryAnsweringService queryAnsweringService, ICostService costService, IDataService dataServicea, IDqService dqService)
        {
            _queryAnsweringService = queryAnsweringService;
            _costService = costService;
            _dataServicea = dataServicea;
            _dqService = dqService;
        }

        /// <summary>
        /// This method decides wheather to create a new sample for the query.
        /// This is pre-execution phase.
        /// </summary>
        public IQueryNode IncludeQueryInTree(IQuery query, out EstimationResult estimationResult)
        {
            //Steps to take:
            //1.Check that query can not be answered from the existing samples.
            //2.Find the appropriate node to hold the sample.
            //3.Create a blank sample.
            //4.Create a node for the sample and put it in the tree. Return the sample.
            PreQueryRunEvent(query);
            estimationResult  = EstimateFromSamples(query);
            PostQueryRunEvent(query);
            if (estimationResult.IfNotNull(er => er.Confidence) < ConfidenceThreshold)
                estimationResult = null;
            if (estimationResult != null)
                return null; //No new sample is created
            
            var parentNode = FindParentNode(Tree, query);
            var newSample = new Sample(TableFactory.CreateTable(query),
                                       false);
            //TODO: When inserting a node, all possible childrens should be detected and moved under the new node
            var newNode = new QueryNode(parentNode, query, new Popularity(1, 1), newSample);
            parentNode.Childs.Add(newNode);

            return newNode;
        }


        internal static IQueryNode FindParentNode(QueryTree tree, IQuery query)
        {
            //Start from root
            //Recursively, find all the containers of the query
            var rv =  FindParentNode(tree.Root, query);
            return rv ?? tree.Root;
        }

        private static IQueryNode FindParentNode(IQueryNode node, IQuery query)
        {
            var child = node.Childs.Where(c => query.IsSubsetOf(c.Query))
                .OrderBy(q => q.Cardinality)
                .FirstOrDefault(q => q.Sample.Materialized);
            var rv = child != null ? FindParentNode(child, query) : node;
            return rv;
        }


        /// <summary>
        /// This method runs the query against the existing tree, and extimates the results for aggregate functions
        /// based on existing samples. If there is no way to estimate results, return null.
        /// </summary>
        /// <param name="query"></param>
        public EstimationResult EstimateFromSamples(IQuery query)
        {
            var rv = _queryAnsweringService.AnswerQyeryFromTree(Tree, query);

            return rv;
            //Now execute DQ metric functions.
            //var mefs =
            //    query.Projections.Select((p, i) => new {i, m = p.GetMetricFunction()}).Where(x => x.m != null).Select(
            //        p => new {p.i, p.m});

        }

        public bool MaterializeSampleFromQuery(IQueryNode sample, IQuery query, ITable result)
        {
            var skiprate = (int) (result.Rows.Count*SamplingRate);
            if (skiprate==0 || (skiprate * 5 > result.Rows.Count ))
            {
                //Result is too small to sample. Re generate the skip rate
                skiprate = result.Rows.Count/5;
            }
            skiprate = Math.Max((int) ((float) result.Rows.Count/Math.Max(5, skiprate)), 1);
            var sampleResult = sample.Sample.Table;//TableFactory.CreateTable(result);
            var filter = result.Rows.Select((r,i) => new {r,i}).Where(x => skiprate == 0 || x.i % skiprate == 0).Select(r => r.r)
                .ToList();

            sampleResult.FillFromFilter(result, filter);

            //Use cost model. Check if can materialize.
            if (_costService.CanMaterialize(sample, query))
            {
                //Update metric functions
                _dqService.UpdateMetricFunctions(query, sampleResult);
                sample.Sample.Materialize();
                return true;
            }
            return false;
        }

        public EstimationResult ExecuteQuery(IQuery query, out ITable result)
        {
            EstimationResult rv;
            var addedNode = IncludeQueryInTree(query, out rv);
            result =  _dataServicea.RunQuery(query);
            if (addedNode != null)
            {
                if (MaterializeSampleFromQuery(addedNode, query, result))
                    SampleMaterialzied(query, addedNode.Sample);
            }
            return rv;
        }

        internal static IEnumerable<IQueryNode> FindAllIntersections(QueryTree tree, IQuery query)
        {
            var theParent = FindParentNode(tree, query);
            return theParent.Childs.Where(s => s.Query.DoesIntersect(query))
                .Concat(FindAllIntersections(tree.Root, query, theParent));
        }

        private static IEnumerable<IQueryNode> FindAllIntersections(IQueryNode node, IQuery query, IQueryNode dontLookNode)
        {
            if (SamplingOptions.Instance.LoockupIntersections)
                return new IQueryNode[] {};
            if (node == dontLookNode)
                return new IQueryNode[] {};
            var childo = node.Childs.Where(s => s.Query.DoesIntersect(query));
            return childo.Concat(childo.SelectMany(s => FindAllIntersections(s, query, dontLookNode)));
        }
    }
}
