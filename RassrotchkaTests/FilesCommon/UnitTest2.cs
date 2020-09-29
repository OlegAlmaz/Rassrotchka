using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rassrotchka;

namespace RassrotchkaTests.FilesCommon
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod1()
        {
            long ident = 0;
            string stringConnection = "Data Source=DC01-10487049;Initial Catalog=Nedoimka;Integrated Security=True";
            string commandText = "SELECT COUNT(Id_dpg) FROM DebitPayGen WHERE Id_dpg = " + ident;
            SqlConnection connection = new SqlConnection(stringConnection);
            connection.Open();
            SqlCommand command = new SqlCommand(commandText, connection);
            int result = (int)command.ExecuteScalar();
            connection.Close();

            Assert.AreEqual(0, result);

        }
    }
}
