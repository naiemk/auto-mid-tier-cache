using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class TestExtraHelpers
    {
        [TestMethod]
        public void GetAllSubsets()
        {
            var items = new[] {"A", "B", "C"};
            var subsets = ExtraHelpers.Sets.SubsetHelper.GetAllSubsets(items).ToArray();

            Assert.AreEqual(7, subsets.Length);
            Assert.AreEqual("A", subsets[0][0]);
            Assert.AreEqual("B", subsets[0][1]);
            Assert.AreEqual("C", subsets[0][2]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetAllSubsetsUpperBound()
        {
            ExtraHelpers.Sets.SubsetHelper.GetAllSubsets(Enumerable.Repeat("A", 100).ToArray());
        }
    }
}
