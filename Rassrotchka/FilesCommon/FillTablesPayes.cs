using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Objects;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Documents;
using Nedoimka;
using Nedoimka.NedoimkaDataSetTableAdapters;
using WriteToSql;
using Arguments = Nedoimka.Arguments;
using GemBox.Spreadsheet;

namespace Nedoimka.FilesCommon
{
	/// <summary>
	/// Класс, заполняющий таблицу выданных рассрочек
	/// </summary>
	public class FillTablesPayes
	{
		public ArgumentDebitPay Argument { get; set; }

		public FillTablesPayes(ArgumentDebitPay arg)
		{
			Argument = arg;
		}

		/// <summary>
		/// заполнение таблиц базы даных из объектов DataTable информацией о рассрочках
		/// </summary>
		/// <returns></returns>
		public string UpdateSqlTableDebitPayGen()
		{
			string mess = "";
			using (var connection = new SqlConnection(Properties.Settings.Default.NedoimkaConnectionString))
			{
				try
				{
					connection.Open();
					SqlCommand command = connection.CreateCommand();
					command.CommandText = string.Format("SELECT * FROM {0}", Argument.TableBase);
					var adapter = new SqlDataAdapter(command);

					var debitPayTable = new DataTable();//таблица из базы данных

					adapter.Fill(debitPayTable);

					using (var debitPayTableGemBox = GetDebitPayTableGemBox())
					{
						ReoderTable(debitPayTable, debitPayTableGemBox);//обновляем объект debitPayTable
					}
					adapter.InsertCommand = CreateInsertCommand(connection);
					int rowCount = adapter.Update(debitPayTable);//обновляем данные  в базе о вынесенных решениях
					mess = string.Format("Обновлено {0} строк.", rowCount);
					return mess;
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}
		}

		public string UpdateSqlTableDebitPayGen2()
		{
			string mess = "";
			var dataSet = new NedoimkaDataSet();
			var adapter1 = new DebitPayGenTableAdapter();
			adapter1.Fill(dataSet.DebitPayGen);
			using (var debitPayTableGemBox = GetDebitPayTableGemBox())
			{
				ReoderTable(dataSet.DebitPayGen, debitPayTableGemBox);//обновляем объект debitPayTable
			}
			int rowCount = adapter1.Update(dataSet.DebitPayGen);//обновляем данные  в базе о вынесенных решениях
			mess = string.Format("Обновлено {0} строк.", rowCount);
			return mess;
		}

		private SqlCommand CreateInsertCommand(SqlConnection connection)
		{
			var command = connection.CreateCommand();
			command.CommandText = @"INSERT INTO DebitPayGen 
									(Id_dpg, Kod_GNI, Name, Kod_Payer, Date_Decis, Numb_Decis, 
									 GniOrGKNS, Summa_Decis, Kod_Paying, Date_first, Date_end, 
									 Count_Mount, Summa_Payer, Type_Decis, Note)
										VALUES (@Id_dpg, @Kod_GNI, @Name, @Kod_Payer, @Date_Decis, @Numb_Decis, 
												@GniOrGKNS, @Summa_Decis, @Kod_Paying, @Date_first, @Date_end, 
												@Count_Mount, @Summa_Payer, @Type_Decis, @Note)";
			SqlParameterCollection pc = command.Parameters;
			pc.Add("@Id_dpg", SqlDbType.BigInt, 0, "Id_dpg");
			pc.Add("@Kod_GNI", SqlDbType.SmallInt, 0, "Kod_GNI");
			pc.Add("@Name", SqlDbType.VarChar, 0, "Name");
			pc.Add("@Kod_Payer", SqlDbType.BigInt, 0, "Kod_Payer");
			pc.Add("@Date_Decis", SqlDbType.DateTime, 0, "Date_Decis");
			pc.Add("@Numb_Decis", SqlDbType.VarChar, 0, "Numb_Decis");
			pc.Add("@GniOrGKNS", SqlDbType.VarChar, 0, "GniOrGKNS");
			pc.Add("@Summa_Decis", SqlDbType.Money, 0, "Summa_Decis");
			pc.Add("@Kod_Paying", SqlDbType.Int, 0, "Kod_Paying");
			pc.Add("@Date_first", SqlDbType.DateTime, 0, "Date_first");
			pc.Add("@Date_end", SqlDbType.DateTime, 0, "Date_end");
			pc.Add("@Count_Mount", SqlDbType.Int, 0, "Count_Mount");
			pc.Add("@Summa_Payer", SqlDbType.Money, 0, "Summa_Payer");
			pc.Add("@Type_Decis", SqlDbType.VarChar, 0, "Type_Decis");
			pc.Add("@Note", SqlDbType.VarChar, 0, "Note");

			return command;
		}


		/// <summary>
		/// заполнение таблиц базы даных из объектов DataTable информацией о ежемесячных платежах
		/// </summary>
		/// <returns></returns>
		public string UpdateSqlTableMonthPay()
		{
			string mess = "";
			using (var connection = new SqlConnection(Properties.Settings.Default.NedoimkaConnectionString))
			{
				try
				{
					connection.Open();
					SqlCommand command = connection.CreateCommand();
					command.CommandText = Argument.SelectedCommand;
					var adapter = new SqlDataAdapter(command);

					var debitPayTable = new DataTable();//таблица из базы данных
					adapter.Fill(debitPayTable);

					command.CommandText = @"DECLARE @IdMax BIGINT
									SET @IdMax = (SELECT MAX(ID_MP) FROM MonthPay)
									SELECT TOP 1 * FROM MonthPay WHERE ID_MP = @IdMax";
					adapter = new SqlDataAdapter(command);
					var monthPayTable = new DataTable();
					adapter.Fill(monthPayTable);//
					GetTableMontPay(debitPayTable, monthPayTable);
					adapter.InsertCommand = CreateInsertCommandMonthPay(connection, Argument.TableBaseMonthPay);
					int rowCount = adapter.Update(monthPayTable);//обновляем данные  в базе о вынесенных решениях
					mess = string.Format("Обновлено {0} строк.", rowCount);
					return mess;
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}

		}

		private SqlCommand CreateInsertCommandMonthPay(SqlConnection connection, string tabName)
		{
			var command = connection.CreateCommand();
			command.CommandText = string.Format(@"INSERT INTO {0} 
									(ID_MP, Id_dpg, Date, Summa_pay)
										VALUES (@ID_MP, @Id_dpg, @Date, @Summa_pay)", tabName);
			SqlParameterCollection pc = command.Parameters;
			pc.Add("@ID_MP", SqlDbType.Int, 0, "ID_MP");
			pc.Add("@Id_dpg", SqlDbType.BigInt, 0, "Id_dpg");
			pc.Add("@Date", SqlDbType.DateTime, 0, "Date");
			pc.Add("@Summa_pay", SqlDbType.Money, 0, "Summa_pay");

			return command;
		}


		private static string ReoderTable(DataTable debitPayTable, DataTable debitPayTableGemBox)
		{
			//var keys = new DataColumn[1];
			//keys[0] = debitPayTable.Columns["Id_dpg"];
			//debitPayTable.PrimaryKey = keys;
			string mess = "";
			var dict = new DictPropName();
			//проверяем по идентификатору есть ли решение о рассрочке либо отсрочке в базе данных
			for (int i = 0; i < debitPayTableGemBox.Rows.Count; i++)
			{
				var ident = Convert.ToInt64(debitPayTableGemBox.Rows[i]["0"]);
				bool notHave;
				notHave = debitPayTable.Rows.Contains(ident);
				if (notHave == false)//если не существует
				{
					DataRow row = debitPayTable.NewRow();
					for (int j = 0; j < debitPayTableGemBox.Columns.Count; j++)
					{
						string colName = debitPayTableGemBox.Columns[j].ColumnName;//имя колонки в таблице excel файла
						string colNameTableBase;//имя колонки в таблице базы данных
						if (dict.TryGetValue(colName, out colNameTableBase))//если есть такая
						{
							object val = debitPayTableGemBox.Rows[i][colName];
							row[colNameTableBase] = val;
						}
					}
					debitPayTable.Rows.Add(row);
				}
				//else//проверяем есть ли изменения данных в существующих в базе строках
				//{
				//    DataRow row = debitPayTable.Rows.Find(ident);//получаем строку c имеющимися в базе данными
				//    int id = debitPayTable.Rows.IndexOf(row);
				//    for (int k = 0; k < debitPayTableGemBox.Columns.Count; k++)
				//    {
				//        string colName = debitPayTableGemBox.Columns[k].ColumnName;//имя колонки в таблице excel файла
				//        string colNameTableBase;//имя колонки в таблице базы данных
				//        if (dict.TryGetValue(colName, out colNameTableBase))//если есть такая
				//        {
				//            //если не равна, то перезаписываем
				//            if (debitPayTable.Rows[id][colNameTableBase] != debitPayTableGemBox.Rows[i][colName])
				//            {
				//                debitPayTable.Rows[id][colNameTableBase] = debitPayTableGemBox.Rows[i][colName];
				//            }
				//        }
				//    }
				//}
			}
			return mess;
		}


		public string Fill(NedoimkaEntities entities, DataTable table)
		{
			string mess;
			try
			{
				//Добавление данных в таблицу DebitPayGen
				List<DebitPayGen> list = GetDebitPayListGemBox<DebitPayGen>(table);
				foreach (DebitPayGen t in list)
				{
					entities.DebitPayGens.AddObject(t);
				}

				//Добавление данных в таблицу MonthPay
				List<MonthPay> listMonthPay = GetMonthPay(list);
				foreach (MonthPay t in listMonthPay)
				{
					entities.MonthPays.AddObject(t);
				}

				mess = "Заполнение данных прошло успешно";
			}
			catch (Exception ex)
			{
				mess = ex.Message;
			}
			return mess;
		}

		#region Извлечение данных из таблицы рассрочек или отсрочек

		/// <summary>
		/// Извлекает из Excel с помощью библиотеки GemBox и метода worksheet.CreateDataTable()
		/// </summary>
		/// <returns>Список решений по рассрочкам</returns>
		public DataTable GetDebitPayTableGemBox()
		{
			// If using Professional version, put your serial key below.
			string license = Argument.ExcelParametrs.License;
			SpreadsheetInfo.SetLicense(license);

			var workbook = ExcelFile.Load(Argument.FilePath);

			// Select the first worksheet from the file.
			int worksheetNumvber = Argument.WorksheetNummber;
			var worksheet = workbook.Worksheets[worksheetNumvber];

			// Create DataTable from an Excel worksheet.
			var dataTable = worksheet.CreateDataTable(new CreateDataTableOptions()
				{
					StartRow = Argument.ExcelParametrs.StartRow,
					StartColumn = Argument.ExcelParametrs.StartColumn,
					NumberOfColumns = Argument.ExcelParametrs.NumberOfColumns,
					NumberOfRows = Argument.ExcelParametrs.NumberOfRows,
					ColumnHeaders = Argument.ExcelParametrs.ColumnHeaders,
					ExtractDataOptions = Argument.ExcelParametrs.ExtractDataOption
				});
			return dataTable;
		}

		/// <summary>
		/// Копирует данные из таблицы в список объектов перед занесением в базу данных
		/// </summary>
		/// <param name="dataTable">таблица данных</param>
		/// <returns>Список решений по рассрочкам</returns>
		private static List<T> GetDebitPayListGemBox<T>(DataTable dataTable) where T : new ()
		{
			var list = new List<T>();
			var dict = new DictPropName();
			dataTable.Columns.Remove("2");//удаление колонки с наименованием плательщика
			dataTable.Columns.Remove("9");//удаление колонки с наименованием платежа

			for (int i = 0; i < dataTable.Rows.Count; i++)
			{
				var t = new T();
				for (int j = 0; j < dataTable.Columns.Count; j++)
				{
					Type type = t.GetType();

					string nameKey = dataTable.Columns[j].ColumnName;
					string name;
					dict.TryGetValue(nameKey, out name);
					if (name != null)
					{
						PropertyInfo prop = type.GetProperty(name);

						object value = Convert.ChangeType(dataTable.Rows[i][j], prop.PropertyType, new CultureInfo("ru-Ru"));
						prop.SetValue(t, value, null);
					}
				}
				list.Add(t);
			}
			return list;
		}

		#region Извлечение из excel файла данные в форме таблицы, используя поставщик OleDB

		/// <summary>
		/// Извлекает из excel файла данные в форме таблицы используя поставщик OleDB
		/// </summary>
		/// <param name="arg"></param>
		/// <returns> возращает объект List/<DebitPayGen/></returns>
		private static List<DebitPayGen> GetDebitPay(ArgumentDebitPay arg)
		{
			const int timeOut = 30;
			string connectionString = ConnectionStringsSetter.ForExcelFiles(arg.FilePath, timeOut);
			try
			{
				using (var connection = new OleDbConnection(connectionString))
				{

					connection.Open();
					OleDbCommand command = connection.CreateCommand();
					command.CommandText = arg.SelectedCommand;
					var list = GetList(command);
					return list;
				}
			}
			catch (OleDbException e) //ошибка подключения
			{
				throw new Exception(e.Message);
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}

		}

		private DataTable GetDebitPayTable(string filePath)
		{
			const int timeOut = 30;
			string connectionString = ConnectionStringsSetter.ForExcelFiles(filePath, timeOut);
			try
			{
				using (var connection = new OleDbConnection(connectionString))
				{

					connection.Open();
					OleDbCommand command = connection.CreateCommand();
					command.CommandText = Argument.SelectedCommand;
					var adapter = new OleDbDataAdapter(command);
					var dataTable = new DataTable();
					adapter.Fill(dataTable);
					return dataTable;
				}
			}
			catch (OleDbException e) //ошибка подключения
			{
				throw new Exception(e.Message);
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}

		}

		private static List<DebitPayGen> GetList(OleDbCommand command)
		{

			var list = new List<DebitPayGen>();
			new List<MonthPay>();
			int startId = 1;
			int stringNumber = 1;
			var debitPay = new DebitPayGen();
			OleDbDataReader reader = command.ExecuteReader();
			while (reader != null && reader.Read())
			{
				try
				{
					debitPay.Id_dpg = startId;
					debitPay.Kod_Payer = (long?) reader.GetInt64(0);
					Int64 kod = reader.GetInt64(0);
					debitPay.Kod_GNI = reader.GetInt16(1);
					debitPay.Kod_Paying = reader.GetInt32(2);
					debitPay.Date_Decis = reader.GetDateTime(3);
					debitPay.Summa_Decis = reader.GetDecimal(4);
					debitPay.Date_first = reader.GetDateTime(5);
					debitPay.Date_end = reader.GetDateTime(6);
					list.Add(debitPay);

					startId++;
					stringNumber++;
				}
				catch (InvalidCastException e)
				{
					reader.Close();
					throw new Exception("Ошибка формата данных в строке - " + stringNumber.ToString() + ". " + e.Message);
				}
			}

			if (reader != null) reader.Close();
			return list;
		}

		#endregion

		#endregion

		#region Формирование списка платежей

		/// <summary>
		/// Формирование перечня ежемесячных платежей
		/// </summary>
		/// <param name="tableMontPay"></param>
		/// <param name="tableDebitPay"></param>
		/// <returns></returns>
		public static DataTable GetTableMontPay(DataTable tableDebitPay, DataTable tableMontPay)
		{
			Int64 index;
			index = tableMontPay.Rows.Count != 0 ? Convert.ToInt64(tableMontPay.Rows[0][0]) + 1 : 1;
			if (tableMontPay.Rows.Count != 0)
				tableMontPay.Rows.RemoveAt(0);//удаляем единственную строку

			for (int i = 0; i < tableDebitPay.Rows.Count; i++)
			{
				if (tableDebitPay.Rows[i]["Date_first"] != null || tableDebitPay.Rows[i]["Date_end"] != null)//если дата первой и последней уплаты не равны нолю
				{
					//Количество платежей
					int payCount = GetPay((DateTime)tableDebitPay.Rows[i]["Date_first"], (DateTime)tableDebitPay.Rows[i]["Date_end"]);

					var sumPay = (Decimal)tableDebitPay.Rows[i]["Summa_Payer"];

					//заполняем список платежей
					for (int j = 0; j < payCount; j++)
					{
						var rowMP = tableMontPay.NewRow();
						rowMP[0] = index;
						rowMP["Id_dpg"] = (Int64)tableDebitPay.Rows[i]["Id_dpg"];
						if (j < payCount - 1)
						{
							rowMP["Summa_Pay"] = sumPay;
							rowMP["Date"] = ((DateTime)tableDebitPay.Rows[i]["Date_first"]).AddMonths(j);
						}
						else
						{
							rowMP["Summa_Pay"] = (Decimal)tableDebitPay.Rows[i]["Summa_Decis"] - sumPay * (payCount - 1);
							rowMP["Date"] = (DateTime)tableDebitPay.Rows[i]["Date_end"];
						}
						tableMontPay.Rows.Add(rowMP);
						index++;
					}
				}
			}
			return tableMontPay;
		}

		private static DataTable GetTablGetTableMontPayForma()
		{
			using (var connection = new SqlConnection(Properties.Settings.Default.NedoimkaConnectionString))
			{
				connection.Open();
				var commandText = @"DECLARE @IdMax BIGINT
									SET @IdMax = (SELECT MAX(ID_MP) FROM MonthPay)
									SELECT TOP 1 * FROM MonthPay WHERE ID_MP = @IdMax";
				var adapter = new SqlDataAdapter(commandText, connection);
				var dataTable = new DataTable();
				adapter.Fill(dataTable);
				return dataTable;
			}
		}


		private static List<MonthPay> GetListMontPay(DebitPayGen debitPay)
		{
			var list = new List<MonthPay>();

			//Количество платежей
			if (debitPay.Date_first != null || debitPay.Date_end != null)
			{
				int payCount = GetPay((DateTime) debitPay.Date_first, (DateTime) debitPay.Date_end);
				//сумма ежемесячного платежа
				if (debitPay.Summa_Payer != null)
				{
					decimal sumPay = (Decimal)debitPay.Summa_Payer;

					//заполняем список платежей
					for (int i = 0; i < payCount; i++)
					{
						var mp = new MonthPay();
						mp.Id_dpg = debitPay.Id_dpg;
						if (i < payCount - 1)
						{
							mp.Summa_pay = sumPay;
							mp.Date = ((DateTime) debitPay.Date_first).AddMonths(i);
						}
						else
						{
							mp.Summa_pay = debitPay.Summa_Decis - sumPay*(payCount - 1);
							mp.Date = debitPay.Date_end;
						}
						list.Add(mp);
					}
				}
			}
			//Вычисление даты

			return list;

		}


		private static int GetPay(DateTime dateFirst, DateTime dateEnd)
		{
			int deltaYar = dateEnd.Year - dateFirst.Year;
			int paysCount = dateEnd.Month - dateFirst.Month + deltaYar*12 + 1;
			return paysCount;
		}

		private static List<MonthPay> GetMonthPay(List<DebitPayGen> list)
		{
			var lisMonthPay = new List<MonthPay>();
			foreach (DebitPayGen t in list)
			{
				List<MonthPay> l = GetListMontPay(t);
				lisMonthPay.AddRange(l);
			}
			return lisMonthPay;
		}

		#endregion
	}

	public class DictPropName : Dictionary<string, string>
	{
		public DictPropName()
		{
			Add("0", "Id_dpg");
			Add("1", "Kod_GNI");
			Add("2", "Name");
			Add("3", "Kod_Payer");
			Add("4", "Date_Decis");
			Add("5", "Numb_Decis");
			Add("51", "GniOrGKNS");
			Add("6", "Summa_Decis");
			Add("7", "Kod_Paying");
			Add("8", "Date_first");
			Add("9", "Date_end");
			Add("10", "Count_Mount");
			Add("11", "Summa_Payer");
			Add("111", "Date_prolong");
			Add("12", "Type_Decis");
			Add("13", "Note");
		}
	}

}

public class ListDebitPay : DebitPayGen
{
	public ListDebitPay()
	{
	}
}

public class ArgumentDebitPay : Arguments
{
	/// <summary>
	/// Полный путь файла
	/// </summary>
	public string FilePath { get; set; }

	/// <summary>
	/// Команда SQL для выборки данных из файла или базы данных
	/// </summary>
	public string SelectedCommand { get; set; }

	/// <summary>
	/// Номер листа в книге файлов, по умолчанию равен 0
	/// </summary>
	public int WorksheetNummber { get; set; }

	/// <summary>
	/// Иия листа в книге файлов, по умолчанию Лист1
	/// </summary>
	public string WorksheetName { get; set; }

	/// <summary>
	/// Параметры необходимые для извлечения данных с листа
	/// </summary>
	public ExcelParametr ExcelParametrs { get; set; }

	/// <summary>
	/// Имя таблицы из базы данных, сведения которой необходимо обновить, по умолчанию DebitPayGen
	/// </summary>
	public string TableBase { get; set; }

	/// <summary>
	/// Имя таблицы из базы данных учету платежей по рассрочке, сведения которой необходимо обновить, по умолчанию MonthPay
	/// </summary>
	public string TableBaseMonthPay { get; set; }

	//Конструктор
	public ArgumentDebitPay()
	{
		FilePath = "";
		SelectedCommand = "";
		WorksheetNummber = 0;
		WorksheetName = "Лист1$";
		ExcelParametrs = new ExcelParametr();
		TableBase = "DebitPayGen";
		TableBaseMonthPay = "MonthPay";
	}

	/// <summary>
	/// Класс, содержащий параметры для изавлечения данных из excel файла
	/// </summary>
	public class ExcelParametr
	{
		/// <summary>
		/// Начальная строка, по умолчанию равно 2
		/// </summary>
		public int StartRow { get; set; }

		/// <summary>
		/// Начальная колонка, по умолчанию равно 0
		/// </summary>
		public int StartColumn { get; set; }

		/// <summary>
		/// Количество расположенных подряд колонок, подлежацих извлечению, по умолчанию равно 17
		/// </summary>
		public int NumberOfColumns { get; set; }

		/// <summary>
		/// Количество расположенных подряд строк, подлежацих извлечению, по умолчанию равно 2
		/// </summary>
		public int NumberOfRows { get; set; }

		/// <summary>
		/// Параметр указывает, является ли первая строка заголовками колонок, по умолчанию true
		/// </summary>
		public bool ColumnHeaders { get; set; }

		/// <summary>
		/// Опции извлечения данных, по умолчанию StopAtFirstEmptyRow (останов при первой пустой строке)
		/// </summary>
		public ExtractDataOptions ExtractDataOption { get; set; }

		/// <summary>
		/// Лицензия библиотеки GemBox
		/// </summary>
		public string License { get; set; }


		public ExcelParametr()
		{
			StartRow = 2;
			StartColumn = 0;
			NumberOfColumns = 17;
			NumberOfRows = 5000;
			ColumnHeaders = true;
			ExtractDataOption = ExtractDataOptions.StopAtFirstEmptyRow;
			License = "EQU2-1KBF-UZ0D-DJ14";
		}
	}
}



