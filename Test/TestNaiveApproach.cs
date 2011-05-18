using System;
using AlgebraToSqlServer;
using AlgebraTree;
using DqMetricSimulator.Core;
using DqMetricSimulator.Dq;
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
            //Run against AdventureWorksLite database.
            var q1 = new BasicQuery(
                new[]
                    {
                        ProjectionItem.CreateFromName<Int32>("ProductId", true),
                        ProjectionItem.CreateFromName<String>("Name", false),
                        ProjectionItem.CreateFromName<String>("Color", false)
                    },
                new[]
                    {
                        SelectionCondition.CreateFromLambda<string>(color => color == "Black")
                    },
                new[] {"SalesLT.Product"}
                );

            var ds = new SimpleSqlDataService("Data Source=.; Initial Catalog=AdventureWorksLT2008; Integrated Security=SSPI");
            var qas = new SimpleQueryAnsweringService(ds);
            var cs = new NaiveCostService();
            var dqs = new SuperSimpleDqService();
            var context = new SapmlingContext(qas, cs, ds, dqs);
            context.Initialize();

            ITable qResult;
            var rv = context.ExecuteQuery(q1, out qResult);
            
            Assert.IsNull(rv);

            var q2 = new BasicQuery(
                new[]
                    {
                        ProjectionItem.CreateFromName<Int32>("ProductId", true),
                        ProjectionItem.CreateFromName<String>("Name", false),
                        ProjectionItem.CreateFromName<String>("Color", false),
                        ProjectionItem.CreateFromBasicMetric("m_Completeness", "ProcuctId")
                    },
                new[]
                    {
                        SelectionCondition.CreateFromLambda<string>(color => color == "Black"),
                        SelectionCondition.CreateFromLambda<string>(productNumber => productNumber == "HL-U509")
                    },
                new[] {"SalesLT.Product"}
                );

            //The result should be from sample.
            ITable q2Result;
            var rv2 = context.ExecuteQuery(q2, out q2Result);
            Assert.IsNotNull(rv2);
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
                        SelectionCondition.CreateFromLambda<int>(year => year <= 2009),
                        SelectionCondition.CreateFromLambda<int>(year => year == 2009)
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
            Assert.IsTrue(q2.DoesIntersect(q3));
            Assert.IsFalse(q1.DoesIntersect(q3));
            
        }

        [TestMethod]
        public void TestSelectionConditions()
        {
            var s1 = SelectionCondition.CreateFromLambda<int>(year => year > 2008);
            var s2 = SelectionCondition.CreateFromLambda<int>(year => year >= 2008);
            var s3 = SelectionCondition.CreateFromLambda<int>(year => year < 2010);
            var s4 = SelectionCondition.CreateFromLambda<int>(year => year <= 2010);
            var s5 = SelectionCondition.CreateFromLambda<int>(year => year == 2009);
            var s6 = SelectionCondition.CreateFromLambda<int>(year => year > 2009);
            var s7 = SelectionCondition.CreateFromLambda<int>(year => year >= 2009);
            var s8 = SelectionCondition.CreateFromLambda<int>(year => year < 2009);
            var s9 = SelectionCondition.CreateFromLambda<int>(year => year <= 2009);

            //Assert subset
            Assert.IsTrue( s5.IsSubsetOf(s1)  );
            Assert.IsTrue( s5.IsSubsetOf(s2)  );
            Assert.IsTrue( s5.IsSubsetOf(s3)  );
            Assert.IsTrue( s5.IsSubsetOf(s4)  );
            Assert.IsFalse( s1.IsSubsetOf(s5) );
            Assert.IsTrue( s1.IsSubsetOf(s2) );
            Assert.IsFalse( s2.IsSubsetOf(s1) );
            Assert.IsTrue( s6.IsSubsetOf(s1));
            Assert.IsTrue( s7.IsSubsetOf(s1));
            Assert.IsTrue(s6.IsSubsetOf(s2));
            Assert.IsTrue(s7.IsSubsetOf(s2));
            Assert.IsTrue(s8.IsSubsetOf(s3));
            Assert.IsTrue(s9.IsSubsetOf(s3));
            Assert.IsTrue(s8.IsSubsetOf(s4));
            Assert.IsTrue(s9.IsSubsetOf(s4));

            //Assert intersect
            Assert.IsTrue(s1.DoesIntersect(s2));
            Assert.IsTrue(s2.DoesIntersect(s1));
            Assert.IsTrue(s3.DoesIntersect(s1));
            Assert.IsTrue(s4.DoesIntersect(s1));
            Assert.IsTrue(s3.DoesIntersect(s2));
            Assert.IsTrue(s4.DoesIntersect(s2));
            Assert.IsTrue(s5.DoesIntersect(s1));
            Assert.IsTrue(s5.DoesIntersect(s2));
            Assert.IsTrue(s5.DoesIntersect(s3));
            Assert.IsTrue(s5.DoesIntersect(s4));
            Assert.IsTrue(s5.DoesIntersect(s1));
            Assert.IsTrue(s2.DoesIntersect(s7));
            Assert.IsTrue(s7.DoesIntersect(s9));
            Assert.IsTrue(s9.DoesIntersect(s7));
            Assert.IsFalse(s6.DoesIntersect(s8));
            Assert.IsFalse(s8.DoesIntersect(s6));
        }
    }
}
