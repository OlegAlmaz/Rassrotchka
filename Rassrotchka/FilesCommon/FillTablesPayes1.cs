using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using GemBox.Spreadsheet;

namespace Rassrotchka.FilesCommon
{
	/// <summary>
	/// Класс, заполняющий таблицу выданных рассрочек
	/// </summary>
	public class FillTablesPayes1
	{
		private readonly NedoimkaDataSet.DebitPayGenDataTable _tableDebitPay;
		private readonly NedoimkaDataSet.MonthPayDataTable _tableMontPay;
		private readonly DictPropName _dict;
		private RowValidationError _validError;
		public ArgumentDebitPay Argument { get; set; }

		//Конструктор
		public FillTablesPayes1(ArgumentDebitPay arg, NedoimkaDataSet.DebitPayGenDataTable tableDebitPay, NedoimkaDataSet.MonthPayDataTable tableMontPay)
		{
			Argument = arg;
			_tableDebitPay = tableDebitPay;
			_tableMontPay = tableMontPay;
			_dict = new DictPropName();
		}

		//Конструктор
		public FillTablesPayes1(ArgumentDebitPay arg)
		{
			Argument = arg;
			_tableDebitPay = new NedoimkaDataSet.DebitPayGenDataTable();
			_tableMontPay = new NedoimkaDataSet.MonthPayDataTable();
			_dict = new DictPropName();
		}

		/// <summary>
		/// Метод обновляет таблицу рассрочек
		/// </summary>
		/// <returns>сообщение об успешности завершения операции</returns>
		public void UpdateSqlTableDebitPayGen()
		{
			try
			{
				using (var debitPayTableGemBox = GetDebitPayTableGemBox())//извлекаем данные из excel файла
				{
					debitPayTableGemBox.AcceptChanges();
					if(ReoderTable(debitPayTableGemBox))//обновляем объект debitPayTable, если нет ошибок
					{// либо соглашаемся на продолжение обновления данных
						var rows = GetRowsDebit();
						if(rows.Count != 0)
							UpdateTableMontPay(rows);
					}
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.Message + " Ошибка в методе: " + e.TargetSite);
			}
		}

		private List<NedoimkaDataSet.DebitPayGenRow> GetRowsDebit()
		{
			var query = from rowDeb in _tableDebitPay.AsEnumerable()
			            join rowMonth in _tableMontPay.AsEnumerable() on rowDeb.Id_dpg equals rowMonth.Id_dpg
				            into leftOuter
			            from monthPayRow in leftOuter.DefaultIfEmpty()
			            where monthPayRow == null
			            select new {rowDeb};
			return query.Select(anonim => anonim.rowDeb).ToList();
		}

		private void UpdateTableMontPay(IEnumerable<NedoimkaDataSet.DebitPayGenRow> rowsDeb)
		{
			long index = _tableMontPay.Rows.Count == 0
				             ? 0
				             : _tableMontPay.Rows.Cast<object>()
				                           .Select((_, i) => Convert.ToInt64(_tableMontPay.Rows[i][0]))
				                           .Concat(new long[] {1})
				                           .Max();
			index++;
			foreach (var genRow in rowsDeb)
			{
				if (genRow.IsDate_firstNull() || genRow.IsDate_endNull())
					continue;//если дата первой и последней уплаты не равны нолю

				//Количество платежей
				int payCount = OperationWithDate.GetPay(genRow.Date_first, genRow.Date_end);

				//заполняем список платежей
				for (int j = 0; j < payCount; j++)
				{
					var rowMontPay = _tableMontPay.NewMonthPayRow();
					rowMontPay.ID_MP = index;
					rowMontPay.Id_dpg = genRow.Id_dpg;
					if (j < payCount - 1)
					{
						rowMontPay.Summa_pay = genRow.Summa_Payer;
						rowMontPay.Date = genRow.Date_first.AddMonths(j);
					}
					else
					{
						rowMontPay.Summa_pay = genRow.Summa_Decis - (genRow.Summa_Payer * (payCount - 1));
						rowMontPay.Date = genRow.Date_end;
					}
					_tableMontPay.Rows.Add(rowMontPay);
					index++;
				}
			}
		}

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
			var dataTable = worksheet.CreateDataTable(new CreateDataTableOptions
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
		/// Добавляет новые данные и обновляет старые в случае изменения
		/// </summary>
		/// <param name="debitPayTableGemBox"></param>
		private bool ReoderTable(DataTable debitPayTableGemBox)
		{
			int i = 0;//индекс строки
			try
			{
				//проверяем по идентификатору есть ли решение о рассрочке либо отсрочке в базе данных
				for (i = 0; i < debitPayTableGemBox.Rows.Count; i++)
				{
					var rowExcel = debitPayTableGemBox.Rows[i];
					long ident;
					bool flag = Int64.TryParse(rowExcel["0"].ToString(), out ident);
					if (flag)
					{
						bool notHave = _tableDebitPay.Rows.Contains(ident);
						if (notHave == false) //если не существует
						{
							//Проверяем запись нового решения в интервале между отставанием от
							//текущей даты на 35 дней и опережением на 6 дней
							if (IsDecisDateNotRange((DateTime)rowExcel["4"]))
							{
								string filter = $"[0] = {ident}";
								debitPayTableGemBox.DefaultView.RowFilter = filter;
								const string message = "Вы хотите добавить в базу данных это решение о рассрочке либо отсрочке?";
								if (VisualErrorRow(debitPayTableGemBox.DefaultView, message)) //спрашиваем
								{
									//Проверяем строку на ошибки и добавляем в таблицу из базы данных
									ValidateAndAddRow(rowExcel);
								}
							}
							else
							{
								//Проверяем строку на ошибки и добавляем в таблицу из базы данных
								ValidateAndAddRow(rowExcel);
							}
						}
						else //проверяем есть ли изменения данных в существующих в базе строках
						{
							var rowDebPay = _tableDebitPay.FindById_dpg(ident);
							UpdateDates(rowDebPay, rowExcel);
						}
					}
					else
					{
						rowExcel.SetColumnError(0, "Ошибка кода идентификатора! Он имеет значение: " + rowExcel["0"].ToString());
						rowExcel.SetAdded();
					}
				}

				if (debitPayTableGemBox.HasErrors)//выводим ошибки при наличии
				{
					debitPayTableGemBox.DefaultView.RowFilter = "";
					var view = debitPayTableGemBox.DefaultView;
					view.RowStateFilter = DataViewRowState.Added;
					return VisualErrorRow(view, "Следующие строки имеют ошибки");
				}
			}
			catch (Exception e)
			{
				i++;
				throw new Exception(e.Message + ". " + e.TargetSite + " строка: " + i);
			}
			return true;
		}

		/// <summary>
		/// Если дата решения вне заданного диапазона: от -35 до +6 дней от текущей даты
		/// </summary>
		/// <param name="dateDecis">дата решения об отсрочке</param>
		/// <returns>возвращает true, если дата вне заданного диапазона</returns>
		private bool IsDecisDateNotRange(DateTime dateDecis)
		{
			DateTime dateMin = DateTime.Now.AddDays(-40);
			DateTime dateMax = DateTime.Now.AddDays(6);
			if (dateDecis >= dateMin && dateDecis <= dateMax)
				return false;
			return true;
		}

		/// <summary>
		/// Проверка новой строки на ошибки и добавление ее в таблицу для обновления базы данных
		/// </summary>
		private void ValidateAndAddRow(DataRow rowExcel)
		{
			_validError = new RowValidationError(rowExcel);
			if (!_validError.ValidationError()) //если ошибка в строке
				return;
			DataRow rowDeb = _tableDebitPay.NewRow();
			for (int j = 0; j < rowExcel.ItemArray.Length; j++)
			{
				string colName = rowExcel.Table.Columns[j].ColumnName; //имя колонки в таблице excel файла
				string colNameTableBase; //имя колонки в таблице базы данных
				if (_dict.TryGetValue(colName, out colNameTableBase)) //если есть такая
				{
					rowDeb[colNameTableBase] = rowExcel[colName];
				}
				rowDeb["Date_prolong"] = rowDeb["Date_end"];
			}
			_tableDebitPay.Rows.Add(rowDeb);
		}

		/// <summary>
		/// Обновление информации уже имеющихся данных таблицы рассрочек
		/// обновляется последняя колонка - Примечания
		/// </summary>
		/// <param name="rowDeb">строк таблицы из базы данных</param>
		/// <param name="rowGemBox">строка из файла excel</param>
		private void UpdateDates(DataRow rowDeb, DataRow rowGemBox)
		{
			const string nmEx = "13"; //имя колонки файла
			const string nmBs = "Note";//имя колонки из таблицы базы данных
			var obBase = rowDeb[nmBs];
			var obFile = rowGemBox[nmEx];

			//если не равна, то перезаписываем
			if (!obBase.Equals(obFile))
			{
				rowDeb[nmBs] = rowGemBox[nmEx];
				rowDeb.SetColumnError(nmBs, "Внесены изменения в данную ячейку");
			}

			#region Старый код

			//for (int k = 0; k < rowGemBox.ItemArray.Length; k++) //проверяем каждую ячейку этой строки на наличие изменений
			//{
			//    string colName = rowGemBox.Table.Columns[k].ColumnName; //имя колонки в таблице excel файла
			//    string colNameTableBase; //имя колонки в таблице базы данных
			//    if (_dict.TryGetValue(colName, out colNameTableBase)) //если есть такая
			//    {
			//        object obBase = rowDeb[colNameTableBase];
			//        Type type = rowDeb[colNameTableBase].GetType();
			//        object obFile = type.FullName != "System.DBNull"
			//                            ? Convert.ChangeType(rowGemBox[colName], type, new CultureInfo("ru-Ru"))
			//                            : rowGemBox[colName];

			//        //если не равна, то перезаписываем
			//        if (!obBase.Equals(obFile))
			//        {
			//            rowDeb[colNameTableBase] = rowGemBox[colName];
			//            rowDeb.SetColumnError(colNameTableBase, @"Внесены изменения в данную ячейку");
			//        }

			//    }
			//}

			#endregion
		}

		/// <summary>
		/// Метод, спрашивает о том, вносить ли в базу данных информацию о рассрочке либо отсрочке
		/// </summary>
		/// <param name="rows">Извлекаемый рядок из таблицы файлы</param>
		/// <param name="mess"></param>
		/// <returns>возвращает true при нажатии на кнопку ДА, иначе - false</returns>
		public bool IsContinue(DataView rows, string mess)
		{
			var window = new WindowDateNoRange
				{
					DataGrid1 = {ItemsSource = rows},
					TextBlockField = {Text = mess},
				};
			var showDialog = window.ShowDialog();
			return showDialog != null && (bool)showDialog;
		}

		public bool VisualErrorRow(DataView view, string mes)
		{
			var form = new Form1 {dataGridView1 = {DataSource = view}, label1 = {Text = mes}};
			var result = form.ShowDialog();
			return result == DialogResult.OK;
		}

		//public string Fill(NedoimkaEntities entities, DataTable table)
		//{
		//    string mess;
		//    try
		//    {
		//        //Добавление данных в таблицу DebitPayGen
		//        List<DebitPayGen> list = GetDebitPayListGemBox<DebitPayGen>(table);
		//        foreach (DebitPayGen t in list)
		//        {
		//            entities.DebitPayGens.AddObject(t);
		//        }

		//        //Добавление данных в таблицу MonthPay
		//        List<MonthPay> listMonthPay = GetMonthPay(list);
		//        foreach (MonthPay t in listMonthPay)
		//        {
		//            entities.MonthPays.AddObject(t);
		//        }

		//        mess = "Заполнение данных прошло успешно";
		//    }
		//    catch (Exception ex)
		//    {
		//        mess = ex.Message;
		//    }
		//    return mess;
		//}

		#region Извлечение данных из таблицы рассрочек или отсрочек

		//private static List<T> GetDebitPayListGemBox<T>(DataTable dataTable) where T : new ()
		//{
		//    var list = new List<T>();
		//    var dict = new DictPropName();
		//    dataTable.Columns.Remove("2");//удаление колонки с наименованием плательщика
		//    dataTable.Columns.Remove("9");//удаление колонки с наименованием платежа

		//    for (int i = 0; i < dataTable.Rows.Count; i++)
		//    {
		//        var t = new T();
		//        for (int j = 0; j < dataTable.Columns.Count; j++)
		//        {
		//            Type type = t.GetType();

		//            string nameKey = dataTable.Columns[j].ColumnName;
		//            string name;
		//            dict.TryGetValue(nameKey, out name);
		//            if (name != null)
		//            {
		//                PropertyInfo prop = type.GetProperty(name);

		//                object value = Convert.ChangeType(dataTable.Rows[i][j], prop.PropertyType, new CultureInfo("ru-Ru"));
		//                prop.SetValue(t, value, null);
		//            }
		//        }
		//        list.Add(t);
		//    }
		//    return list;
		//}

		#region Извлечение из excel файла данные в форме таблицы, используя поставщик OleDB

		///// <summary>
		///// Извлекает из excel файла данные в форме таблицы используя поставщик OleDB
		///// </summary>
		///// <param name="arg"></param>
		///// <returns> возращает объект List/<DebitPayGen/></returns>
		//private static List<DebitPayGen> GetDebitPay(ArgumentDebitPay arg)
		//{
		//    const int timeOut = 30;
		//    string connectionString = ConnectionStringsSetter.ForExcelFiles(arg.FilePath, timeOut);
		//    try
		//    {
		//        using (var connection = new OleDbConnection(connectionString))
		//        {

		//            connection.Open();
		//            OleDbCommand command = connection.CreateCommand();
		//            command.CommandText = arg.SelectedCommand;
		//            var list = GetList(command);
		//            return list;
		//        }
		//    }
		//    catch (OleDbException e) //ошибка подключения
		//    {
		//        throw new Exception(e.Message);
		//    }
		//    catch (Exception e)
		//    {
		//        throw new Exception(e.Message);
		//    }

		//}

		////private DataTable GetDebitPayTable(string filePath)
		////{
		////    const int timeOut = 30;
		////    string connectionString = ConnectionStringsSetter.ForExcelFiles(filePath, timeOut);
		////    try
		////    {
		////        using (var connection = new OleDbConnection(connectionString))
		//        {

		//            connection.Open();
		//            OleDbCommand command = connection.CreateCommand();
		//            command.CommandText = Argument.SelectedCommand;
		//            var adapter = new OleDbDataAdapter(command);
		//            var dataTable = new DataTable();
		//            adapter.Fill(dataTable);
		//            return dataTable;
		//        }
		//    }
		//    catch (OleDbException e) //ошибка подключения
		//    {
		//        throw new Exception(e.Message);
		//    }
		//    catch (Exception e)
		//    {
		//        throw new Exception(e.Message);
		//    }

		//}

		//private static List<DebitPayGen> GetList(OleDbCommand command)
		//{

		//    var list = new List<DebitPayGen>();
		//    new List<MonthPay>();
		//    int startId = 1;
		//    int stringNumber = 1;
		//    var debitPay = new DebitPayGen();
		//    OleDbDataReader reader = command.ExecuteReader();
		//    while (reader != null && reader.Read())
		//    {
		//        try
		//        {
		//            debitPay.Id_dpg = startId;
		//            debitPay.Kod_Payer = (long?) reader.GetInt64(0);
		//            Int64 kod = reader.GetInt64(0);
		//            debitPay.Kod_GNI = reader.GetInt16(1);
		//            debitPay.Kod_Paying = reader.GetInt32(2);
		//            debitPay.Date_Decis = reader.GetDateTime(3);
		//            debitPay.Summa_Decis = reader.GetDecimal(4);
		//            debitPay.Date_first = reader.GetDateTime(5);
		//            debitPay.Date_end = reader.GetDateTime(6);
		//            list.Add(debitPay);

		//            startId++;
		//            stringNumber++;
		//        }
		//        catch (InvalidCastException e)
		//        {
		//            reader.Close();
		//            throw new Exception("Ошибка формата данных в строке - " + stringNumber.ToString() + ". " + e.Message);
		//        }
		//    }

		//    if (reader != null) reader.Close();
		//    return list;
		//}

		#endregion

		#endregion

		#region Формирование списка платежей

		//private static List<MonthPay> GetListMontPay(DebitPayGen debitPay)
		//{
		//    var list = new List<MonthPay>();

		//    //Количество платежей
		//    if (debitPay.Date_first != null || debitPay.Date_end != null)
		//    {
		//        int payCount = GetPay((DateTime) debitPay.Date_first, (DateTime) debitPay.Date_end);
		//        //сумма ежемесячного платежа
		//        if (debitPay.Summa_Payer != null)
		//        {
		//            decimal sumPay = (Decimal)debitPay.Summa_Payer;

		//            //заполняем список платежей
		//            for (int i = 0; i < payCount; i++)
		//            {
		//                var mp = new MonthPay();
		//                mp.Id_dpg = debitPay.Id_dpg;
		//                if (i < payCount - 1)
		//                {
		//                    mp.Summa_pay = sumPay;
		//                    mp.Date = ((DateTime) debitPay.Date_first).AddMonths(i);
		//                }
		//                else
		//                {
		//                    mp.Summa_pay = debitPay.Summa_Decis - sumPay*(payCount - 1);
		//                    mp.Date = debitPay.Date_end;
		//                }
		//                list.Add(mp);
		//            }
		//        }
		//    }
		//    //Вычисление даты

		//    return list;

		//}

		//private static List<MonthPay> GetMonthPay(List<DebitPayGen> list)
		//{
		//    var lisMonthPay = new List<MonthPay>();
		//    foreach (DebitPayGen t in list)
		//    {
		//        List<MonthPay> l = GetListMontPay(t);
		//        lisMonthPay.AddRange(l);
		//    }
		//    return lisMonthPay;
		//}

		#endregion
	}
}
