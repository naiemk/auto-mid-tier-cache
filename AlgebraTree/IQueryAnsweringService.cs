﻿using System;
using DqMetricSimulator.Core;
using DqMetricSimulator.Query;

namespace AlgebraTree
{
    public interface IQueryAnsweringService
    {
        EstimationResult AnswerQyeryFromTree(QueryTree tree, IQuery query);
    }

    public interface IQueryNodeAnsweringService
    {
        EstimationResult AnswerQueryFromNode(IQueryNode node, IQuery query);
    }

    public class SimpleQueryAnsweringService:IQueryAnsweringService, IQueryNodeAnsweringService
    {
        private readonly IDataService _dataService;

        public SimpleQueryAnsweringService(IDataService dataService)
        {
            _dataService = dataService;
        }

        public EstimationResult AnswerQyeryFromTree(QueryTree tree, IQuery query)
        {
            var node = SapmlingContext.FindParentNode(tree, query);

            if (node == tree.Root)
                return null;
            //Now answer query from node
            return AnswerQueryFromNode(node, query);
        }

        public EstimationResult AnswerQueryFromNode(IQueryNode node, IQuery query)
        {
            var tbl = _dataService.RunQueryAgainstNode(query, node);
            var conf = 1.0f - 1.0f/tbl.Rows.Count;
            var rv = new EstimationResult(tbl, conf);
            return rv;
        }
    }
}
