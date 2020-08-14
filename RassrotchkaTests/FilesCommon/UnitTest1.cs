//Тестирование окна проверки сумм платежей по решению

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rassrotchka;

namespace RassrotchkaTests.FilesCommon
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var window = new WindowValidSumPayers();
            window.Show();
        }
    }
}
