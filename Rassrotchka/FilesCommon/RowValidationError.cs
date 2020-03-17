using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Rassrotchka.FilesCommon
{
	public class RowValidationError
	{
		/// <summary>
		/// Таблица из файла excel
		/// </summary>
		public DataTable TableFile { get; set; }

		protected bool Flag { get; private set; }

		public RowValidationError()
		{
			TableFile = new DataTable();
			Flag = true;
		}

		public bool ValidationError(DataRow row)
		{
			//проверка всех дат
			DatesValidation(row);

			PaysCounValidation(row);
			return Flag;
		}

		//проверка дат решения, а также первой и последней уплаты на соответствие
		private void DatesValidation(DataRow row)
		{
			//Проверка даты первой уплаты
			var dateDecis = (DateTime) row["4"]; //дата решения
			var dateFirst = (DateTime) row["8"]; //дата первой уплаты
			var dateEnd = (DateTime) row["9"]; //дата последней уплаты

			//сравниваем, в следующем ли месяце будет дата первой уплаты рассрочки
			bool flag = (string) row["12"] == "рассрочка" && dateDecis.AddMonths(1).Month == dateFirst.Month;
			if (!(flag))
				AddErrorToTable(row, @"Ошибка в дате первого платежа по рассрочке!");

			//Сравниваем не меньше либо равна дата уплаты по рассрочке по отношению к дате решения
			flag = (string) row["12"] == "отсрочка" && dateDecis < dateFirst;
			if (!flag)
				AddErrorToTable(row, @"Ошибка в дате первого платежа по отсрочке!");

			//проверка даты последней уплаты при отсрочке
			flag = (string) row["12"] == "отсрочка" && dateEnd != dateFirst;
			if (!flag)
				AddErrorToTable(row, @"Ошибка в дате последнего платежа по отсрочке!");

			//проверка даты последней уплаты при рассрочке
			flag = (string) row["12"] == "рассрочка" && dateEnd > dateFirst;
			if (!flag)
				AddErrorToTable(row, @"Ошибка в дате последнего платежа по рассрочке!");
		}

		private void PaysCounValidation(DataRow row)
		{
			//throw new NotImplementedException();
		}


		private void AddErrorToTable(DataRow row, string messError)
		{
			DataRow newRow = TableFile.NewRow();
			if (row.HasErrors == false)
			{
				row.RowError = messError;
				for (int i = 0; i < row.Table.Columns.Count; i++)
				{
					newRow[i] = row[i];
				}
				TableFile.Rows.Add(newRow);
			}
			else
			{
				row.RowError = row.RowError + string.Format("\t{0}", messError);
				//DataRow rowTemp = TableFile.Rows.Find(row["0"]);
				int ind = TableFile.Rows.IndexOf(TableFile.Rows.Find(row["0"]));
				TableFile.Rows[ind].RowError = row.RowError;
			}
			Flag = false;
		}
	}
}
