using System;
using System.Collections.Generic;
using System.Linq;
using DqMetricSimulator.IO;
using DqMetricSimulator.Query;
using DqMetricSimulator.QueryFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class TestCsvQueryFactory
    {
        private class TestIoProvider : IIoProvider
        {
            public Func<string[]> Reader { get; set; }
            public string[] ReadAllLines()
            {
                return Reader();
            }

            public void SetSource(string fileName)
            {
            }
        }

        [TestMethod]
        public void EqualitySelectionCondition()
        {
            var eq = SelectionCondition.Equality("a", "val");
            Assert.AreEqual("(a == \"val\")", eq.Expression.ToString());
            Assert.AreEqual(eq.Parameters.Count, 1);

            string indirect = "val";
            eq = SelectionCondition.Equality("a", indirect);
            Assert.AreEqual("(a == \"val\")", eq.Expression.ToString());
        }

        [TestMethod]
        public void OneUnTypedQueryIsCreated()
        {
            var ioProv = new TestIoProvider() { Reader = () => new[] {"A,B,C,D,E", "0,1,2,3,4"}};
            var factory = new CsvQueryFactory(ioProv);

            var query = factory.Create(new Dictionary<string, string> {{"FileName", ""}, {"Table", "Table"}}).First();

            Assert.AreEqual(query.Projections.Count, 5);
            Assert.IsTrue(query.Projections.ToList()[0].Columns.Contains("A"));
            Assert.AreEqual("(A == \"0\")", query.SelectionConditions.First().Expression.ToString());
            Assert.AreEqual(5, query.SelectionConditions.Count);
        }

        [TestMethod]
        public void OneUntypedQueryWithNull()
        {
            var ioProv = new TestIoProvider() { Reader = () => new[] {"A,B,C,D,E", ",1,,3,"}};
            var factory = new CsvQueryFactory(ioProv);

            var query = factory.Create(new Dictionary<string, string> {{"FileName", ""}, {"Table", "Table"}}).First();

            Assert.AreEqual(query.Projections.Count, 5);
            Assert.IsTrue(query.Projections.ToList()[0].Columns.Contains("A"));
            Assert.AreEqual("(B == \"1\")", query.SelectionConditions.First().Expression.ToString());
            Assert.AreEqual(2, query.SelectionConditions.Count);
        }

        [TestMethod]
        public void OneTypedQuery()
        {
            var ioProv = new TestIoProvider() { Reader = () => new[] {"A:int,B,C:int,D:string,E", "0,,2,3,4"}};
            var factory = new CsvQueryFactory(ioProv);

            var query = factory.Create(new Dictionary<string, string> {{"FileName", ""}, {"Table", "Table"}}).First();

            Assert.AreEqual(query.Projections.Count, 5);
            Assert.IsTrue(query.Projections.ToList()[0].Columns.Contains("A"));
            Assert.AreEqual("(A == 0)", query.SelectionConditions.First().Expression.ToString());
            Assert.AreEqual("(E == \"4\")", query.SelectionConditions.Last().Expression.ToString());
            Assert.AreEqual(4, query.SelectionConditions.Count);
        }
    }
}
