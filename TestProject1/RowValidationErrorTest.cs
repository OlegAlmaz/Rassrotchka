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
					FilePath = 
							@"d:\мои документы\visual studio 2010\Projects\Rassrotchka\TestProject1\TestedFiles\рассрочки.xlsx",
					ExcelParametrs = {StartRow = 2}
				};
			var payes = new FillTablesPayes(arg);
			var target = new RowValidationError(); //
			DataTable table = payes.GetDebitPayTableGemBox();
			table.AcceptChanges();
			var col = table.Columns["0"];
			DataColumn[] columns = new[] {col};
			table.PrimaryKey = columns;
			target.TableFile = table.Clone();
			object idRow = 2111748;//код тестируемого решения
			DataRow row = table.Rows.Find(idRow); // получаем искомый для проверки рядок

			bool actual;
			actual = target.ValidationError(row);
			payes.VisualErrorRow(table);
			//string mess = "Данная строка имеет ошибки";
			//payes.IsContinue(target.TableFile.DefaultView, mess);
			
			Assert.IsTrue(actual);
		}

		/// <summary>
		///Тест для ValidationError
		/// Проверка правильности количества платежей рассрочки
		///</summary>
		[TestMethod()]
		public void ValidationErrorTest_1()
		{
			var arg = new ArgumentDebitPay
			{
				FilePath =
						@"d:\мои документы\visual studio 2010\Projects\Rassrotchka\TestProject1\TestedFiles\рассрочки.xlsx",
				ExcelParametrs = { StartRow = 2 }
			};
			var payes = new FillTablesPayes(arg);
			var target = new RowValidationError(); //
			DataTable table = payes.GetDebitPayTableGemBox();
			table.AcceptChanges();
			var col = table.Columns["0"];
			DataColumn[] columns = new[] { col };
			table.PrimaryKey = columns;
			//target.TableFile = table.Clone();
			object idRow = 4735942;//код тестируемого решения
			DataRow row = table.Rows.Find(idRow); // получаем искомый для проверки рядок

			bool actual;
			actual = target.ValidationError(row);
			payes.VisualErrorRow(table);
			//string mess = "Данная строка имеет ошибки";
			//payes.IsContinue(target.TableFile.DefaultView, mess);

			Assert.IsTrue(actual);
		}

		/// <summary>
		///Тест для CodePayersValidation
		///</summary>
		[TestMethod()]
		public void CodePayersValidationTest1()
		{
			var arg = new ArgumentDebitPay
			{
				FilePath =
						@"d:\мои документы\visual studio 2010\Projects\Rassrotchka\TestProject1\TestedFiles\рассрочки.xlsx",
				ExcelParametrs = { StartRow = 2 }
			};
			var payes = new FillTablesPayes(arg);
			DataTable table = payes.GetDebitPayTableGemBox();
			var col = table.Columns["0"];
			DataColumn[] columns = new[] { col };
			table.PrimaryKey = columns;
			object idRow = 4735943;//код тестируемого решения
			DataRow row = table.Rows.Find(idRow); // получаем искомый для проверки рядок

			var target = new RowValidationError(); // 
			bool actual;
			actual = target.CodePayersValidation(row);
	
			Assert.IsTrue(actual);


		}

		/// <summary>
		///Тест для CodePaymentsValidation
		///</summary>
		[TestMethod()]
		public void CodePaymentsValidationTest()
		{
			var arg = new ArgumentDebitPay
			{
				FilePath =
						@"d:\мои документы\visual studio 2010\Projects\Rassrotchka\TestProject1\TestedFiles\рассрочки.xlsx",
				ExcelParametrs = { StartRow = 2 }
			};
			var payes = new FillTablesPayes(arg);
			DataTable table = payes.GetDebitPayTableGemBox();
			var col = table.Columns["0"];
			DataColumn[] columns = new[] { col };
			table.PrimaryKey = columns;
			object idRow = 4735944;//код тестируемого решения
			DataRow row = table.Rows.Find(idRow); // получаем искомый для проверки рядок

			var target = new RowValidationError(); // 
			bool actual;
			actual = target.CodePaymentsValidation(row);

			Assert.IsTrue(actual);

		}
	}
}
