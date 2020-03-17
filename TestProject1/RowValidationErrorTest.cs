using Rassrotchka.FilesCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace TestProject1
{
    
    
    /// <summary>
    ///Это класс теста для RowValidationErrorTest, в котором должны
    ///находиться все модульные тесты RowValidationErrorTest
    ///</summary>
	[TestClass()]
	public class RowValidationErrorTest
	{


		private TestContext testContextInstance;

		/// <summary>
		///Получает или устанавливает контекст теста, в котором предоставляются
		///сведения о текущем тестовом запуске и обеспечивается его функциональность.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Дополнительные атрибуты теста
		// 
		//При написании тестов можно использовать следующие дополнительные атрибуты:
		//
		//ClassInitialize используется для выполнения кода до запуска первого теста в классе
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//TestInitialize используется для выполнения кода перед запуском каждого теста
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//TestCleanup используется для выполнения кода после завершения каждого теста
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		/// <summary>
		///Тест для ValidationError
		/// Проверка правильности введенных дат в строке решения о рассрочке
		///</summary>
		[TestMethod()]
		public void ValidationErrorTest()
		{
			var arg = new ArgumentDebitPay
				{
					FilePath = @"d:\мои документы\visual studio 2010\Projects\Rassrotchka\TestProject1\TestedFiles\рассрочки.xlsx",
					ExcelParametrs = {StartRow = 2}
				};
			var payes = new FillTablesPayes(arg);
			var target = new RowValidationError(); //
			DataTable table = payes.GetDebitPayTableGemBox();
			var col = table.Columns["0"];
			DataColumn[] columns = new[] {col};
			table.PrimaryKey = columns;
			target.TableFile = table.Clone();
			object idRow = 2111748;//код тестируемого решения
			DataRow row = table.Rows.Find(idRow); // получаем искомый для проверки рядок
			bool expected = false;
			bool actual;
			actual = target.ValidationError(row);
			string mess = "Данная строка имеет ошибки";
			payes.IsContinue(target.TableFile.DefaultView, mess);
			Assert.AreEqual(expected, actual);
		}
	}
}
