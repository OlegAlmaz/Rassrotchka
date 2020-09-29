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

		private DataRow _rowEcel;

		public DateTime dateDecis { get; }
		public DateTime dateFirst { get; }
		public DateTime dateEnd { get; }
		public decimal sumDecis { get; }
		public string typDec { get; }//тип решения отсрочка либо рассрочка

		#endregion
		/// <summary>
		/// конструктор
		/// </summary>
		/// <param name="rowExcel">Проверяемая строка из excel таблицы</param>
		public RowValidationError(DataRow rowExcel)
		{
			_rowEcel = rowExcel;

			dateDecis = (DateTime)_rowEcel[DtDec]; //дата решения
			dateFirst = (DateTime)_rowEcel[DtFrs]; //дата первой уплаты
			dateEnd = (DateTime)_rowEcel[DtEnd]; //дата последней уплаты
			sumDecis = Convert.ToDecimal(_rowEcel[SumDcs].ToString());//cумма по решению
			typDec = (string)_rowEcel[TypDec];

		}

		/// <summary>
		/// уведомление об отсутствии ошибки если значение равно true
		/// </summary>
		public bool IsNotError { get; private set; }

		/// <summary>
		/// проверка строки, извлеченной из файла excel
		/// </summary>
		/// <param name="rowExcel"></param>
		/// <returns>возвращает true, если нет ошибки, и наоборот</returns>
		public bool ValidationError()
		{
			IsNotError = true;
			//проверка на пустые ячейки
			IsNotError = IsNotEmtyValidation();
			if (IsNotError == false)
			{
				_rowEcel.RowError = "Ошибка в строке";
				_rowEcel.SetAdded();
				return IsNotError;
			}

			//проверка кода плательщика налогов
			if (CodePayersValidation() == false)
				IsNotError = false;

			//проверка кода платежа
			if (CodePaymentsValidation() == false)
				IsNotError = false;

			//проверка всех дат
			if (DatesValidation() == false)
				IsNotError = false;

			if (PaysCounValidation() == false)
				IsNotError = false;

			if (IsNotError == false)
			{
				_rowEcel.RowError = "Ошибка в строке";
				_rowEcel.SetAdded();
			}
			return IsNotError;
		}

		//проверка на пустые ячейки
		private bool IsNotEmtyValidation()
		{
			bool isNotError = true;//нет пустых ячеек
			int indexOf = _rowEcel.Table.Rows.IndexOf(_rowEcel);
			for (int i = 0; i < _rowEcel.ItemArray.Length; i++)
			{
				//заполняем колонку "Орган принявший решение" в случае, если она пустая
				string clmnName = _rowEcel.Table.Columns[i].ColumnName;
				if (clmnName == "51")
				{
					Type type = _rowEcel.Table.Rows[indexOf][i].GetType();
					if (type.FullName == "System.DBNull")
					{
						_rowEcel[clmnName] = "ГНИ";
						_rowEcel.AcceptChanges();
					}
				}
				if (clmnName != "71" && clmnName != "13" && clmnName != "111")
				{
					Type type = _rowEcel.Table.Rows[indexOf][i].GetType();
					if (type.FullName == "System.DBNull")
					{
						_rowEcel.SetColumnError(i, @"Недопустима пустая ячейка");
						isNotError = false;
					}
				}
			}
			return isNotError;
		}

		//проверка кода платежа
		public bool CodePaymentsValidation()
		{
			bool isNotError = true;//нет ошибок
			Int64 code = Convert.ToInt64(_rowEcel[CdPayng]);
			const string @select = @"SELECT Kod_Pay FROM Payments";
			DataTable table = GetCodes(select);
			if (table.Rows.Contains(code) == false)//если не содержится
			{
				isNotError = false;
				_rowEcel.SetColumnError(CdPayng, @"Такой код платежа отсутствует в базе данных.");
			}

			return isNotError;
		}

		//Проверка кода плательщика налогов
		public bool CodePayersValidation()
		{
			bool isNotError = true;//нет ошибок
			Int64 code = Convert.ToInt64(_rowEcel[CdPaer]);
			string @select = code <= 99999999
				                 ? @"SELECT KOD FROM Name_Plat WHERE KOD IS NOT NULL"
				                 : @"SELECT [Код плательщика] FROM Name_Plat_fiz WHERE [Код плательщика] IS  NOT NULL";
			DataTable table = GetCodes(select);
			if (table.Rows.Contains(code) == false)
			{
				isNotError = false;
				_rowEcel.SetColumnError(CdPaer, @"Плательщик с таким кодом отсутствует в базе данных.");
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
		private bool DatesValidation()
		{
			bool isNotError = true;//если нет ошибок

			if (typDec == "рассрочка")
			{
				//сравниваем, в следующем ли месяце будет дата первой уплаты рассрочки
				if (dateFirst.Month < dateDecis.AddMonths(1).Month)
				{
					_rowEcel.SetColumnError(DtFrs, "Ошибка в дате первого платежа по рассрочке!");
					isNotError = false;
				}
				//проверка даты последней уплаты при рассрочке
				if (dateEnd <= dateFirst)
				{
					_rowEcel.SetColumnError(DtEnd, "Ошибка в дате последнего платежа по рассрочке!");
					isNotError = false;
				}
			}

			if (typDec == "отсрочка")
			{
				//Сравниваем не меньше либо равна дата уплаты по отсрочке по отношению к дате решения
				if (dateFirst <= dateDecis)
				{
					_rowEcel.SetColumnError(DtFrs, "Ошибка в дате первого платежа по отсрочке!");
					isNotError = false;
				}

				//проверка даты последней уплаты при рассрочке
				if (dateEnd != dateFirst)
				{
					_rowEcel.SetColumnError(DtEnd, "Ошибка в дате последнего платежа по отсрочке!");
					isNotError = false;
				}
			}
			return isNotError;
		}

		//Проверка количества платежей в случае рассрочки проверка значенич колоники "10".
		//Проверка значения суммы ежемесячного платежа (колонка "11").
		private bool PaysCounValidation()
		{
			bool isNotError = true;//если нет ошибок

			//=========Проверка количества платежей

			//расчетное количество месяцев отсрочки либо рассрочки
			int expected = (dateEnd.Year - dateDecis.Year)*12 + (dateEnd.Month - dateDecis.Month);

			//фактическое количество месяцев отсрочки либо рассрочки
			int actual = Convert.ToInt32(_rowEcel[PayCnt].ToString());

			if (actual != expected)
			{
				_rowEcel.SetColumnError(PayCnt, "Ошибка в количестве месяцев действия рассрочки либо отсрочки, правильное значение: " + expected);
				isNotError = false;
			}

			//===============Проверка суммы ежемесячного платежа
			decimal sumPayActual = Convert.ToDecimal(_rowEcel[SumPay].ToString()); // заявленная сумма ежемесячного платежа

			if (typDec == "отсрочка")
			{
				if (sumPayActual != sumDecis)
				{
					_rowEcel.SetColumnError(SumPay, "Ошибка в сумме ежемесячного платежа, правильное значение: " + sumDecis);
					isNotError = false;
				}
			}
			else if (typDec == "рассрочка")// рассрочка
			{
				int payCount = OperationWithDate.GetPay(dateFirst, dateEnd);//количество платежей расчетное
				//положительное значение дата первого платежа равна либо больше даты последнего платежа
				if (payCount > 0)
				{
					decimal sumPayExpected = sumDecis / payCount; //расчетное значение

					if (Math.Abs(sumPayExpected - sumPayActual) >= 1000)
					{
						_rowEcel.SetColumnError(SumPay, "Ошибка в cумме ежемесячного платежа, расчетное значение: " + sumPayExpected);
						isNotError = false;
					}
				}
				//отрицательное значение в случае, если дата первой уплаты больше даты последего платежа
				else
				{
					_rowEcel.SetColumnError(SumPay, "Проверить cумму ежемесячного платежа невозможно в связи с тем, что неверно указаны даты (дата первого платежа больше даты последнего платежа)");
					isNotError = false;
				}

			}
			else
			{
				_rowEcel.SetColumnError(TypDec, "Ошибка в наименовании типа решения, д.б отсрочка либо рассрочка");
				isNotError = false;
			}

			return isNotError;
		}
	}
}
