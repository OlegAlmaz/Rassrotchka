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
    public class WindowValidSumPayersTests
    {
        [TestMethod()]
        public void WindowValidSumPayersTest()
        {
            var win = new WindowValidSumPayers();
            win.ShowDialog();
            bool actual = win.IsLoaded;
            actual = win.IsVisible;
            Assert.IsFalse(actual);
        }
    }
}