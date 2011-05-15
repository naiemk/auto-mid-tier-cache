using System;
using AlgebraToSqlServer;
using AlgebraTree;
using DqMetricSimulator.Core;
using DqMetricSimulator.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    /// <summary>
    /// Summary description for TestNaiveApproach
    /// </summary>
    [TestClass]
    public class TestNaiveApproach
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion



        [TestMethod]
        public void AddThreeQueries()
        {
            var q1 = new BasicQuery(
                new[]
                    {
                        ProjectionItem.CreateFromName<Int32>("Id", true),
                        ProjectionItem.CreateFromName<String>("Name", false)
                    },
                new[]
                    {
                        SelectionCondition.CreateFromLambda<string>(conferenceName => conferenceName == "VLDB")
                    },
                new[] {"Sources"}
                );

            var ds = new SimpleSqlDataService();
            var qas = new SimpleQueryAnsweringService(ds);
            var cs = new NaiveCostService();
            var context = new SapmlingContext(qas, cs, ds);
            context.Initialize();

            ITable qResult;
            var rv = context.ExecuteQuery(q1, out qResult);

            Assert.IsNotNull(rv);
        }

        [TestMethod]
        public void TestMergeableAndSubSet()
        {
            var q1 = new BasicQuery(
                new[]
                    {
                        ProjectionItem.CreateFromName<Int32>("Id", true),
                        ProjectionItem.CreateFromName<String>("Name", false)
                    },
                new[]
                    {
                        SelectionCondition.CreateFromLambda<string>(conferenceName => conferenceName == "VLDB"),
                        SelectionCondition.CreateFromLambda<int>(year => year >= 2009),
                        SelectionCondition.CreateFromLambda<int>(year => year <= 2009)
                    },
                new[] {"Sources"}
                );
            var q2 = new BasicQuery(
                new[]
                    {
                        ProjectionItem.CreateFromName<Int32>("Id", true),
                        ProjectionItem.CreateFromName<String>("Name", false)
                    },
                new[]
                    {
                        SelectionCondition.CreateFromLambda<int>(year => year >= 2008),
                        SelectionCondition.CreateFromLambda<int>(year => year <= 2010)
                    },
                new[] {"Sources"}
                );
            var q3 = new BasicQuery(
                new[]
                    {
                        ProjectionItem.CreateFromName<Int32>("Id", true),
                        ProjectionItem.CreateFromName<String>("Name", false),
                        ProjectionItem.CreateFromName<String>("NewColumn", false)
                    },
                new[]
                    {
                        SelectionCondition.CreateFromLambda<int>(year => year >= 2009),
                        SelectionCondition.CreateFromLambda<String>(conferenceName => conferenceName == "ICKM")
                    },
                new[] {"Sources"}
                );

            Assert.IsTrue(q1.IsSubsetOf(q2));
            Assert.IsFalse(q2.IsSubsetOf(q1));
            Assert.IsTrue(q3.DoesIntersect(q2));
            //Assert.IsTrue(q2.DoesIntersect(q3));
            Assert.IsFalse(q1.DoesIntersect(q3));
            
        }
    }
}
