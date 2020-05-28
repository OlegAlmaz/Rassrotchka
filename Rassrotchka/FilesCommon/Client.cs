using System.Data;
using System.Data.SqlClient;
using GemBox.Spreadsheet;
using Rassrotchka.FilesCommon;
using Rassrotchka.Properties;

namespace Rassrotchka
{
	public class Client
	{
		private readonly AbstractTable _table;
		private readonly AbstractFile _file;
		public Client(AbstractFactory factory)
		{
			_table = factory.CreateTable();
			_file = factory.CreateFile();
		}

		public void Run(ArgumentDebitPay args)
		{
			_table.Args = args;
			_file.Interact(_table);
		}

	}

	public abstract class AbstractFactory
	{
		public abstract AbstractTable CreateTable();
		public abstract AbstractFile CreateFile();
	}

	public abstract class AbstractTable
	{
		public ArgumentDebitPay Args;

		public DataSet SqlToDataSet()
		{
			var sqlConnection = new SqlConnection(Settings.Default.NedoimkaConnectionString);
			using (var dataSet = new DataSet())
			{
				//заполнение сводной таблицы
				var sqlDataAdapter = SqlDataAdapter(sqlConnection);
				sqlDataAdapter.Fill(dataSet);
				return dataSet;
			}
		}

		protected abstract SqlDataAdapter SqlDataAdapter(SqlConnection sqlConnection);
	}

	public abstract class AbstractFile
	{
		public string FilePath { get; set; }
		public string NewFile { get; set; }

		//public FilterOPF FilterOpf;
		public abstract void Interact(AbstractTable tb);

		/// <summary>
		/// Формирует и копирует данные в excel файл на одном листе
		/// </summary>
		/// <param name="tb"></param>
		protected void DateToFile(AbstractTable tb)
		{
			string licensi = tb.Args.ExcelParametrs.License;
			SpreadsheetInfo.SetLicense(licensi);
			var workbook = ExcelFile.Load(FilePath);
			var ws = workbook.Worksheets[0];
			DataTable dataTable = tb.SqlToDataSet().Tables[0];
			var options = new InsertDataTableOptions(TableCellsNames.CellData);
			ws.InsertDataTable(dataTable, options);
			ws.Cells[HeaderFile.NameCellHeader].Value = HeaderFile.Header;
			FormattingRangeGemBox.FormattingUsedRange(ws, TableCellsNames.CellData);
			workbook.Save(NewFile);
		}

		/// <summary>
		/// Содержит содержание заголовка файла и имя ячейки, в которое он помещается
		/// </summary>
		protected static class HeaderFile
		{
			public static string Header = string.Empty;
			/// <summary>
			/// имя ячейки в которую помещается заголовок файла, по умолчанию "A1"
			/// </summary>
			public static string NameCellHeader = "A1";
		}

		/// <summary>
		/// Содержит имена ячеек, в которых помещаются названия колонок таблицы 
		/// и непосредственно данные таблицы
		/// </summary>
		protected static class TableCellsNames
		{
			/// <summary>
			/// первая ячейка строки с названиями колонок, по умолчанию - "A2"
			/// </summary>
			public static string HeaderCollumns = "A2";
			/// <summary>
			/// первая ячейка таблицы с данными, по умолчанию - "A3"
			/// </summary>
			public static string CellData = "A3";
		}
	}


}
