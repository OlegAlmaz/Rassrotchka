using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Rassrotchka.FilesCommon
{
	public class RowValidationError
	{
		private const string DtDec = "4"; //колонка даты решения
		private const string DtFrs = "8"; //колонка даты решения
		private const string DtEnd = "9"; //колонка даты решения
		private const string TypDec = "12"; //колонка типа решения (отсрочка либо отсрочка)
		private const string PayCnt = "10";//колонка количества платежей рассрочки
		private const string SumDcs = "6"; //колонка cуммы по решение о рассрочке или отсрочке
		private const string SumPay = "11"; //колонка cуммы по решение о рассрочке или отсрочке

		/// <summary>
		/// Таблица из файла excel
		/// </summary>
		public DataTable TableFile { get; set; }

		/// <summary>
		/// уведомление об ошибке исли значение равно true
		/// </summary>
		public bool IsError { get; private set; }

		public RowValidationError()
		{
			TableFile = new DataTable();
			IsError = true;
		}

		public bool ValidationError(DataRow row)
		{
			//проверка всех дат
			IsError = DatesValidation(row);

			IsError = PaysCounValidation(row);

			if (IsError)
			{
				row.RowError = @"Ошибка в строке";
				row.SetModified();
			}
			return IsError;
		}

		//проверка дат решения, а также первой и последней уплаты на соответствие
		//Если ошика в строке, получаем значение true
		private bool DatesValidation(DataRow row)
		{
			bool isError = false;//если нет ошибок

			var dateDecis = (DateTime) row[DtDec]; //дата решения
			var dateFirst = (DateTime) row[DtFrs]; //дата первой уплаты
			var dateEnd = (DateTime) row[DtEnd]; //дата последней уплаты

			//===========Проверка даты первой уплаты

			//сравниваем, в следующем ли месяце будет дата первой уплаты рассрочки
			if ((string)row[TypDec] == "рассрочка" && dateDecis.AddMonths(1).Month != dateFirst.Month)
			{
				row.SetColumnError(DtFrs, @"Ошибка в дате первого платежа по рассрочке!");
				isError = true;
			}
			//Сравниваем не меньше либо равна дата уплаты по отсрочке по отношению к дате решения
			if ((string)row[TypDec] == "отсрочка" && dateFirst <= dateDecis)
			{
				row.SetColumnError(DtFrs, @"Ошибка в дате первого платежа по отсрочке!");
				isError = true;
			}

			//===========Проверка даты последней уплаты

			//проверка даты последней уплаты при отсрочке
			if ((string)row[TypDec] == "отсрочка" && dateEnd != dateFirst)
			{
				row.SetColumnError(DtEnd, @"Ошибка в дате последнего платежа по отсрочке!");
				isError = true;
			}
			//проверка даты последней уплаты при рассрочке
			if ((string)row[TypDec] == "рассрочка" && dateEnd <= dateFirst)
			{
				row.SetColumnError(DtEnd, @"Ошибка в дате последнего платежа по рассрочке!");
				isError = true;
			}
			return isError;
		}

		//Проверка количества платежей в случае рассрочки проверка значенич колоники "10".
		//Проверка значения суммы ежемесячного платежа (колонка "11").
		private bool PaysCounValidation(DataRow row)
		{
			bool isError = false;//если нет ошибок
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
				isError = true;

			}			

			//===============Проверка суммы ежемесячного платежа
			decimal sumPayExpected = sumDecis/expected; //расчетное значение
			decimal sumPayActual = Convert.ToDecimal(row[SumPay].ToString()); // по факту
			if (Math.Abs(sumPayExpected - sumPayActual) >= 1000)
			{
				row.SetColumnError(SumPay, @"Ошибка в cумме ежемесячного платежа, расчетное значение: " + sumPayExpected);
				isError = true;
			}
			return isError;
		}
	}
}
