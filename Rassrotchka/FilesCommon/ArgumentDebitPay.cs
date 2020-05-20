using System;
using GemBox.Spreadsheet;

namespace Rassrotchka.FilesCommon
{
	public class ArgumentDebitPay
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

		/// <summary>
		/// Дата на которую готовится сводный отчет
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		/// Имя хранимой процедуры
		/// </summary>
		public string ProcedureName { get; set; }

		/// <summary>
		/// Начальная дата в диапазоне выбранных дат
		/// </summary>
		public DateTime DateFirst { get; set; }

		/// <summary>
		/// Конечная дата в диапазоне выбранных дат
		/// </summary>
		public DateTime DateEnd { get; set; }

		/// <summary>
		/// Название месяца в заголовке файла
		/// </summary>
		public MonthName Monthname { get; set; }

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
			Monthname = new MonthName("", "");
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
}