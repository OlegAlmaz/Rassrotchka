using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Rassrotchka;
using Rassrotchka.FilesCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using Rassrotchka.NedoimkaDataSetTableAdapters;

namespace TestProject1
{
    
    
    /// <summary>
    ///Это класс теста для FillTablesPayesTest, в котором должны
    ///находиться все модульные тесты FillTablesPayesTest
    ///</summary>
	[TestClass()]
	public class FillTablesPayesTest
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

		private bool IsNotValuesEqual(object a, object b)
		{
			Type t = a.GetType();
			if (t == Type.GetType("System.DBNull") && b.GetType() == Type.GetType("System.DBNull"))
				return false;
			if (t == Type.GetType("System.DBNull") && b.GetType() != Type.GetType("System.DBNull"))
				return true;
			if (t == Type.GetType("System.DateTime"))
			{
				int d = DateTime.Compare((DateTime) a, (DateTime) b);
				if (d == 0)
					return false;
				return true;
			}
			if (t == Type.GetType("System.String"))
			{
				int s = String.Compare(a.ToString(), b.ToString(), StringComparison.CurrentCultureIgnoreCase);
				if (s == 0)
					return false;
				return true;
			}
			if (Equals(Convert.ToDouble(a), Convert.ToDouble(b)))
				return false;
			return true;
		}

		/// <summary>
		/// Обновление информации уже имеющихся данных таблицы рассрочек
		/// </summary>
		/// <param name="debitPayTable">таблица из базы данных</param>
		/// <param name="debitPayTableGemBox">таблица с данными из файла excel</param>
		/// <param name="ident">код решения о рассрочке </param>
		/// <param name="rowNumb">номер рядка таблицы debitPayTableGemBox</param>
		private void UpdateDates(DataTable debitPayTable, DataTable debitPayTableGemBox, long ident, int rowNumb)
		{
			var dict = new DictPropName();

			DataRow row = debitPayTable.Rows.Find(ident); //получаем строку c имеющимися в базе данными
			int id = debitPayTable.Rows.IndexOf(row); //получаем индекс данной строки в таблице из базы данных
			for (int k = 0; k < debitPayTableGemBox.Columns.Count; k++) //проверяем каждую ячейку этой строки на наличие изменений
			{
				string colName = debitPayTableGemBox.Columns[k].ColumnName; //имя колонки в таблице excel файла
				string colNameTableBase; //имя колонки в таблице базы данных
				if (dict.TryGetValue(colName, out colNameTableBase)) //если есть такая
				{
					object bas = debitPayTable.Rows[id][colNameTableBase];
					object file = debitPayTableGemBox.Rows[rowNumb][colName];
					//если не равна, то спрашиваем о перезаписи
					if (IsNotValuesEqual(bas, file))
					{
						string inform1 = string.Format("\r Были: код {0:F0}; имя {1}; дата решения {2:d}; сумма по решению {3:N2}; измененные данные \"{4}\""
							, debitPayTable.Rows[id]["Kod_Payer"]
							, debitPayTable.Rows[id]["Name"]
							, debitPayTable.Rows[id]["Date_Decis"]
							, debitPayTable.Rows[id]["Summa_Decis"]
							, debitPayTable.Rows[id][colNameTableBase]);
						string inform2 = string.Format("\r Стали: код {0:F0}; имя {1}; дата решения {2:d}; сумма по решению {3:N2}; измененные данные \"{4}\""
							, debitPayTableGemBox.Rows[rowNumb]["3"]
							, debitPayTableGemBox.Rows[rowNumb]["2"]
							, debitPayTableGemBox.Rows[rowNumb]["4"]
							, debitPayTableGemBox.Rows[rowNumb]["6"]
							, debitPayTableGemBox.Rows[rowNumb][colName]);

						MessageBoxResult result = MessageBox.Show("Обновить данные? " + inform1 + inform2, "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Information);
						if (result == MessageBoxResult.OK)//перезаписываем
						{
							debitPayTable.Rows[id][colNameTableBase] = debitPayTableGemBox.Rows[rowNumb][colName];
						}
					}
				}
			}
		}
		//-----------------------------------------


		/// <summary>
		///Тест для UpdateDates
		///</summary>
		[TestMethod()]
		public void UpdateDatesTest()
		{
			var arg = new ArgumentDebitPay()
			{
				FilePath = @"d:\Мои документы\Visual Studio 2010\Projects\Rassrotchka\TestProject3\TestedFiles\рассрочки.xlsx"
			};
			var target = new FillTablesPayes1(arg);
			var debitPayTable = new NedoimkaDataSet.DebitPayGenTestDataTable();
			var adapter = new DebitPayGenTestTableAdapter();
			adapter.Fill(debitPayTable);

			DataTable debitPayTableGemBox = target.GetDebitPayTableGemBox();
			long ident = 158769;
			int i = 0;
			UpdateDates(debitPayTable, debitPayTableGemBox, ident, i);

			object expected = new DateTime(2021, 12, 19);
			object actual = debitPayTable.Rows[1]["Date_prolong"];
			Assert.AreEqual(expected, actual);

			expected = "58 ";
			actual = debitPayTable.Rows[1]["Numb_Decis"];
			Assert.AreEqual(expected, actual);

			expected = 8117.86m;
			actual = debitPayTable.Rows[1]["Summa_Payer"];
			Assert.AreEqual(expected, actual);

			expected = DBNull.Value;
			actual = debitPayTable.Rows[1]["Note"];
			Assert.AreEqual(expected, actual);
		}

		#region Тестирование метода IsDecisDateNotRange(DateTime dateDecis)
		/// <summary>
		/// Проверяет дату решения о рассрочке на вхождениие в интервал
		/// от 35 дней до текущей даты и 6 дней от нее
		/// </summary>
		/// <param name="dateDecis">дата решения об отсрочке</param>
		/// <returns>возвращает true, если дата в нужном диапазоне</returns>
		private bool IsDecisDateNotRange(DateTime dateDecis)
		{
			DateTime dateMin = DateTime.Now.AddDays(-35);
			DateTime dateMax = DateTime.Now.AddDays(6);
			if (dateDecis >= dateMin && dateDecis <= dateMax )
				return false;
			return true;
		}

	    
		[TestMethod]
		public void ValidateDecisDateTest()
		{
			DateTime date = DateTime.Now.AddDays(-36);
			Assert.AreEqual(true, IsDecisDateNotRange(date));

			date = DateTime.Now.AddDays(-35);
			Assert.AreEqual(false, IsDecisDateNotRange(date));

			date = DateTime.Now.AddDays(7);
			Assert.AreEqual(true, IsDecisDateNotRange(date));
		}

	    #endregion

		/// <summary>
		///Тест для UpdateSqlTableDebitPayGen для нетестовой базы
		///</summary>
		[TestMethod()]
		public void UpdateSqlTableDebitPayGenTest2()
		{
			int gni = 36;
			string filePath =
				string.Format(@"d:\Мои документы\Рассрочки\!Учет поступлений по рассрочке\Контроль\рассрочки_{0}_2020_03.xlsx", gni);
			var arg = new ArgumentDebitPay()
			{
				FilePath = @"d:\Мои документы\Рассрочки\!Учет поступлений по рассрочке\рассрочки_00_2020_03.xlsx"
			};
			var target = new FillTablesPayes1(arg);
			var debitPayTable = new NedoimkaDataSet.DebitPayGenDataTable();
			var adapter = new DebitPayGenTableAdapter();
			adapter.Fill(debitPayTable);

			bool expected = true; // TODO: инициализация подходящего значения
			bool actual = true;
			target.UpdateSqlTableDebitPayGen(debitPayTable);
			int countRow = 0;
			if (actual == true)
				adapter.Update(debitPayTable);
			MessageBox.Show("Обновлено " + countRow + " строк") ;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///Тест для UpdateSqlTableDebitPayGen
		///</summary>
		[TestMethod()]
		public void UpdateSqlTableDebitPayGenTest()
		{
			var arg = new ArgumentDebitPay()
			{
				FilePath = @"d:\Мои документы\Visual Studio 2010\Projects\Rassrotchka\TestProject1\TestedFiles\рассрочки.xlsx"
			};
			var target = new FillTablesPayes1(arg);
			var debitPayTable = new NedoimkaDataSet.DebitPayGenTestDataTable();
			var adapter = new DebitPayGenTestTableAdapter();
			adapter.Fill(debitPayTable);

			bool expected = true; // TODO: инициализация подходящего значения
			bool actual = true;
			target.UpdateSqlTableDebitPayGen(debitPayTable);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///Тест для IsContinue
		///</summary>
		[TestMethod()]
		public void IsContinueTest()
		{
			var arg = new ArgumentDebitPay()
			{
				FilePath = @"d:\Мои документы\Visual Studio 2010\Projects\Rassrotchka\TestProject1\TestedFiles\рассрочки.xlsx"
			};
			var target = new FillTablesPayes1(arg);
			DataTable debitPayTableGemBox = target.GetDebitPayTableGemBox();
			long id = 158769;
			string expression = string.Format("[0] = {0}", id);
			IEnumerable<DataRow> rows = debitPayTableGemBox.Select(expression);
			DataTable table = rows.CopyToDataTable();
			table.DefaultView.RowFilter = "[0] = " + id;
			string mess = "Вы хотите добавить эту строку";
			Assert.AreEqual(true, target.IsContinue(table.DefaultView, mess));//нажимаем кнопку "Да"
			Assert.AreEqual(false, target.IsContinue(table.DefaultView, mess));//нажимаем кнопку "Нет"
		}

		/// <summary>
		///Тест для VisualAddedRows
		///</summary>
		[TestMethod()]
		public void VisualAddedRowsTest()
		{
			var arg = new ArgumentDebitPay()
			{
				FilePath = @"d:\Мои документы\Visual Studio 2010\Projects\Rassrotchka\TestProject1\TestedFiles\рассрочки.xlsx"
			};
			var target = new FillTablesPayes1(arg);

			var debitPayTable = new NedoimkaDataSet.DebitPayGenTestDataTable();
			var adapter = new DebitPayGenTestTableAdapter();
			adapter.Fill(debitPayTable);
			using (var debitPayTableGemBox = target.GetDebitPayTableGemBox())
			{
				for (int i = 0; i < debitPayTableGemBox.Rows.Count; i++)
				{
					var ident = Convert.ToInt64(debitPayTableGemBox.Rows[i]["0"]);
					bool notHave = debitPayTable.Rows.Contains(ident);
					if (notHave == false) //если не существует
					{
//						target.AddRow(debitPayTable, debitPayTableGemBox, i);//todo изменить
					}
				}
			}
			DataView view = debitPayTable.DefaultView;
			view.RowStateFilter = DataViewRowState.Added;
//			FillTablesPayes.VisualAddedRows(view);//todo изменить
			int expected = 5;
			int actual = debitPayTable.Select("", "", DataViewRowState.Added).Length;
			Assert.AreEqual(expected, actual);
		}
	}
}
