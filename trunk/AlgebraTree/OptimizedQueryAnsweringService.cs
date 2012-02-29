using System;
using System.Linq;
using DqMetricSimulator.Data;
using DqMetricSimulator.Query;

namespace AlgebraTree
{
    public class OptimizedQueryAnsweringService : IQueryAnsweringService
    {
        private readonly IDataService _dataService;
        private SimpleQueryAnsweringService _simpleQaService;

        public OptimizedQueryAnsweringService(IDataService dataService)
        {
            _dataService = dataService;
            _simpleQaService = new SimpleQueryAnsweringService(dataService);
        }

        public EstimationResult AnswerQyeryFromTree(QueryTree tree, IQuery query)
        {
            throw new NotImplementedException();
            //Find all nodes that may answer a part of query
            //in addition to the single node that totally satisfies the query
            var theParent = SapmlingContext.FindParentNode(tree, query);
            var theParentResult = _simpleQaService.AnswerQueryFromNode(theParent, query);
            var allIntersectors = SapmlingContext.FindAllIntersections(tree, query);
            var intersectionResults = allIntersectors.Select(s => _simpleQaService.AnswerQueryFromNode(s, query));

            var initialResult = _simpleQaService.AnswerQyeryFromTree(tree, query);
            if (initialResult.Confidence > 0.8)
            {
                //If there is a single parent node, try to optimize the query result using all intersections
                var allRows = theParentResult.Result.Rows.Union(intersectionResults.SelectMany(rs => rs.Result.Rows));
                var resultTable = TableFactory.CreateTable(theParentResult.Result, allRows);
            }
            else
            {
                //There is no single node to satisfy the query, do the best guess
            }
        }
    }
}
