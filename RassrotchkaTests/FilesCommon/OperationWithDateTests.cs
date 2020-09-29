using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rassrotchka.FilesCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rassrotchka.FilesCommon.Tests
{
    [TestClass()]
    public class OperationWithDateTests
    {
        DateTime first;
        DateTime end;

        [TestMethod()]
        public void GetPayTest()
        {
            first = new DateTime(2020, 10, 1);
            end = new DateTime(2020, 10, 1);
            
            int expexted = 1;

            Assert.AreEqual(expexted, OperationWithDate.GetPay(first, end));
        }

        [TestMethod()]
        public void GetPayTest2()
        {
            first = new DateTime(2020, 10, 1);
            end = new DateTime(2020, 10, 2);

            int expexted = 1;

            Assert.AreEqual(expexted, OperationWithDate.GetPay(first, end));
        }

        [TestMethod()]
        public void GetPayTest3()
        {
            first = new DateTime(2020, 10, 1);
            end = new DateTime(2020, 11, 2);

            int expexted = 2;

            Assert.AreEqual(expexted, OperationWithDate.GetPay(first, end));
        }

        [TestMethod()]
        public void GetPayTest4()
        {
            first = new DateTime(2020, 11, 1);
            end = new DateTime(2020, 10, 2);

            int expexted = -2;

            Assert.AreEqual(expexted, OperationWithDate.GetPay(first, end));
        }

        [TestMethod()]
        public void GetPayTest5()
        {
            first = new DateTime(2021, 1, 1);
            end = new DateTime(2020, 12, 25);

            int expexted = -2;

            Assert.AreEqual(expexted, OperationWithDate.GetPay(first, end));
        }
        [TestMethod()]
        public void GetPayTest6()
        {
            first = new DateTime(2020, 12, 25);
            end = new DateTime(2021, 01, 01);

            int expexted = 2;

            Assert.AreEqual(expexted, OperationWithDate.GetPay(first, end));
        }

        [TestMethod()]
        public void GetPayTest7()
        {
            first = new DateTime(2020, 11, 2);
            end = new DateTime(2020, 11, 1);

            int expexted = -1;

            Assert.AreEqual(expexted, OperationWithDate.GetPay(first, end));
        }


    }
}