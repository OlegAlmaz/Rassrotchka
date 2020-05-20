using System;
using System.Data;
using System.Data.SqlClient;

namespace Rassrotchka.FilesCommon
{
	public class RowValidationError
	{
		#region закрытые поля

		private const string CdPaer = "3"; //колонка кодов плательщиков налогов
		private const string CdPayng = "7"; //колонка кодов вида платежа
		private const string DtDec = "4"; //колонка даты решения
		private const string DtFrs = "8"; //колонка даты первой уплаты
		private const string DtEnd = "9"; //колонка даты последней уплаты
		private const string TypDec = "12"; //колонка типа решения (отсрочка либо отсрочка)
		private const string PayCnt = "10"; //колонка количества платежей рассрочки
		private const string SumDcs = "6"; //колонка cуммы по решение о рассрочке или отсрочке
		private const string SumPay = "11"; //колонка cуммы ежемесячного платежа

		#endregion
		
		/// <summary>
		/// уведомление об отсутствии ошибки если значение равно true
		/// </summary>
		public bool IsNotError { get; private set; }

		/// <summary>
		/// проверка строки, извлеченной из файла excel
		/// возвращает true, если нет ошибки, и наоборот
		/// </summary>
		/// <param name="rowExcel"></param>
		/// <returns></returns>
		public bool ValidationError(DataRow rowExcel)
		{
			IsNotError = true;
			//проверка на пустые ячейки
			IsNotError = IsNotEmtyValidation(rowExcel);
			if (IsNotError == false)
			{
				rowExcel.RowError = @"Ошибка в строке";
				rowExcel.SetAdded();
				return IsNotError;
			}

			//проверка кода плательщика налогов
			if (CodePayersValidation(rowExcel) == false)
				IsNotError = false;

			//проверка кода платежа
			if (CodePaymentsValidation(rowExcel) == false)
				IsNotError = false;

			//проверка всех дат
			if (DatesValidation(rowExcel) == false)
				IsNotError = false;

			if (PaysCounValidation(rowExcel) == false)
				IsNotError = false;

			if (IsNotError == false)
			{
				rowExcel.RowError = @"Ошибка в строке";
				rowExcel.SetAdded();
			}
			return IsNotError;
		}

		//проверка на пустые ячейки
		private bool IsNotEmtyValidation(DataRow row)
		{
			bool isNotError = true;//нет пустых ячеек
			int indexOf = row.Table.Rows.IndexOf(row);
			for (int i = 0; i < row.ItemArray.Length; i++)
			{
				//заполняем колонку "Орган принявший решение" в случае, если она пустая
				string clmnName = row.Table.Columns[i].ColumnName;
				if (clmnName == "51")
				{
					Type type = row.Table.Rows[indexOf][i].GetType();
					if (type.FullName == "System.DBNull")
					{
						row[clmnName] = "ГНИ";
						row.AcceptChanges();
					}
				}
				if (clmnName != "71" && clmnName != "13" && clmnName != "111")
				{
					Type type = row.Table.Rows[indexOf][i].GetType();
					if (type.FullName == "System.DBNull")
					{
						row.SetColumnError(i, @"Недопустима пустая ячейка");
						isNotError = false;
					}
				}
			}
			return isNotError;
		}

		//проверка кода платежа
		public bool CodePaymentsValidation(DataRow row)
		{
			bool isNotError = true;//нет ошибок
			Int64 code = Convert.ToInt64(row[CdPayng]);
			const string @select = @"SELECT Kod_Pay FROM Payments";
			DataTable table = GetCodes(select);
			if (table.Rows.Contains(code) == false)//если не содержится
			{
				isNotError = false;
				row.SetColumnError(CdPayng, @"Такой код платежа отсутствует в базе данных.");
			}

			return isNotError;
		}

		//Проверка кода плательщика налогов
		public bool CodePayersValidation(DataRow row)
		{
			bool isNotError = true;//нет ошибок
			Int64 code = Convert.ToInt64(row[CdPaer]);
			string @select = code <= 99999999
				                 ? @"SELECT KOD FROM Name_Plat WHERE KOD IS NOT NULL"
				                 : @"SELECT [Код плательщика] FROM Name_Plat_fiz WHERE [Код плательщика] IS  NOT NULL";
			DataTable table = GetCodes(select);
			if (table.Rows.Contains(code) == false)
			{
				isNotError = false;
				row.SetColumnError(CdPaer, @"Плательщик с таким кодом отсутствует в базе данных.");
			}
			return isNotError;

		}

		private DataTable GetCodes(string selectCommad)
		{
			var conn = Properties.Settings.Default.NedoimkaConnectionString;
			using (var connection = new SqlConnection(conn))
			{
				connection.Open();
				var adapter = new SqlDataAdapter(selectCommad, connection);
				var table = new DataTable();
				adapter.Fill(table);
				DataColumn column = table.Columns[0];
				var columns = new[] {column};
				table.PrimaryKey = columns;
				return table;
			}
		}

		//проверка дат решения, а также первой и последней уплаты на соответствие
		//Если ошика в строке, получаем значение true
		private bool DatesValidation(DataRow row)
		{
			bool isNotError = true;//если нет ошибок

			var dateDecis = (DateTime) row[DtDec]; //дата решения
			var dateFirst = (DateTime) row[DtFrs]; //дата первой уплаты
			var dateEnd = (DateTime) row[DtEnd]; //дата последней уплаты

			//===========Проверка даты первой уплаты
			//сравниваем, в следующем ли месяце будет дата первой уплаты рассрочки
			if ((string)row[TypDec] == "рассрочка" && dateDecis.AddMonths(1).Month != dateFirst.Month)
			{
				row.SetColumnError(DtFrs, @"Ошибка в дате первого платежа по рассрочке!");
				isNotError = false;
			}
			//Сравниваем не меньше либо равна дата уплаты по отсрочке по отношению к дате решения
			if ((string)row[TypDec] == "отсрочка" && dateFirst <= dateDecis)
			{
				row.SetColumnError(DtFrs, @"Ошибка в дате первого платежа по отсрочке!");
				isNotError = false;
			}

			//===========Проверка даты последней уплаты

			//проверка даты последней уплаты при отсрочке
			if ((string)row[TypDec] == "отсрочка" && dateEnd != dateFirst)
			{
				row.SetColumnError(DtEnd, @"Ошибка в дате последнего платежа по отсрочке!");
				isNotError = false;
			}
			//проверка даты последней уплаты при рассрочке
			if ((string)row[TypDec] == "рассрочка" && dateEnd <= dateFirst)
			{
				row.SetColumnError(DtEnd, @"Ошибка в дате последнего платежа по рассрочке!");
				isNotError = false;
			}

			return isNotError;
		}

		//Проверка количества платежей в случае рассрочки проверка значенич колоники "10".
		//Проверка значения суммы ежемесячного платежа (колонка "11").
		private bool PaysCounValidation(DataRow row)
		{
			bool isNotError = true;//если нет ошибок
			var dateDecis = (DateTime) row[DtDec]; //дата решения
			var dateEnd = (DateTime) row[DtEnd]; //дата первой уплаты
			var sumDecis = Convert.ToDecimal(row[SumDcs].ToString());//cумма по решению

			//=========Проверка количества платежей

			//должно быть количество месяце отсрочки либо рассрочки (количество платеже отсрочки)
			int expected = (dateEnd.Year - dateDecis.Year)*12 + (dateEnd.Month - dateDecis.Month);

			int actual = Convert.ToInt32(row[PayCnt].ToString());// по факту

			if (actual != expected)
			{
				row.SetColumnError(PayCnt, @"Ошибка в количестве месяцев действия рассрочки либо отсрочки");
				isNotError = false;
			}

			//===============Проверка суммы ежемесячного платежа
			decimal sumPayActual = Convert.ToDecimal(row[SumPay].ToString()); // по факту
			if ((string)row[TypDec] == "отсрочка")
			{
				if (sumPayActual != sumDecis)
				{
					row.SetColumnError(SumPay, @"Ошибка в сумме ежемесячного платежа, правильное значение: " + sumDecis);
					isNotError = false;
				}
			}
			else// рассрочка
			{
				decimal sumPayExpected = sumDecis / expected; //расчетное значение

				if (Math.Abs(sumPayExpected - sumPayActual) >= 1000)
				{
					row.SetColumnError(SumPay, @"Ошибка в cумме ежемесячного платежа, расчетное значение: " + sumPayExpected);
					isNotError = false;
				}
			}
			return isNotError;
		}
	}
}
