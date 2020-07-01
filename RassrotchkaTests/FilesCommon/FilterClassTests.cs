using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rassrotchka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rassrotchka.Tests
{
    [TestClass()]
    public class FilterClassTests
    {
        [TestMethod()]
        public void FilterStringTest()
        {
            FilterClass filter = new FilterClass();
            filter.Close = "true";
            filter.Kod_GNI = "2";
            string expected = "WHERE Close = true AND Kod_GNI = 2";

            Assert.AreEqual(expected, filter.FilterString());
        }

        [TestMethod()]
        public void FilterStringTest2()
        {
            FilterClass filter = new FilterClass();
            filter.Close = "true";
            filter.Name = "ЛЭС";
            string expected = "WHERE Close = true AND Name LIKE %ЛЭС%";

            Assert.AreEqual(expected, filter.FilterString());
        }
        [TestMethod()]
        public void FilterStringTest3()
        {
            FilterClass filter = new FilterClass();
            filter.Close = "true";
            filter.Kod_Payer = "1111";
            string expected = "WHERE Close = true AND Kod_Payer = 1111";

            Assert.AreEqual(expected, filter.FilterString());
        }
        [TestMethod()]
        public void FilterStringTest4()
        {
            FilterClass filter = new FilterClass();
            filter.Close = "true";
            filter.Kod_GNI = "2";
            filter.Name = "ЛЭС";
            string expected = "WHERE Close = true AND Kod_GNI = 2 AND Name LIKE %ЛЭС%";

            Assert.AreEqual(expected, filter.FilterString());
        }
    }
}