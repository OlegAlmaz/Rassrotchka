using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using GemBox.Spreadsheet;
using ClosedXML.Excel;

namespace Rassrotchka
{
	/// <summary>
	/// формирует сводные данные по рассрочке на определенную дату
	/// </summary>
	public class FactoryConsolid : AbstractFactory
	{
		public override AbstractTable CreateTable()
		{
			return new TableConsolid();
		}

		public override AbstractFile CreateFile()
		{
			return new FileConsolid();
		}
	}

	public class TableConsolid : AbstractTable
	{
		protected override SqlDataAdapter SqlDataAdapter(SqlConnection sqlConnection)
		{
			var adapter = new SqlDataAdapter(BaseElementName.ProcedConsolid, sqlConnection)
			{
				SelectCommand = { CommandType = CommandType.StoredProcedure }
			};
			return adapter;
		}
	}

	public class FileConsolid : AbstractFile
	{
		public override void Interact(AbstractTable tb)
		{
			FilePath = "FilesTemplates\\рассрочки_2020_03.xlsx";
			TableCellsNames.HeaderCollumns = "A3";
			TableCellsNames.CellData = "A4";
			HeaderFile.NameCellHeader = "A1";
			DateToFileTwoSheets(tb);
		}

		/// <summary>
		/// Формирует и копирует данные в excel файл на двух листах
		/// </summary>
		/// <param name="tb"></param>
		private void DateToFileTwoSheets(AbstractTable tb)
		{
			string licensi = tb.Args.ExcelParametrs.License;
			SpreadsheetInfo.SetLicense(licensi);
			var workbook = ExcelFile.Load(FilePath);
			var dataSet = tb.SqlToDataSet();

			//заполняем первый лист - свод решений рассрочек
			var ws = workbook.Worksheets[0];
			DataTable dataTable = dataSet.Tables[0];
			var options = new InsertDataTableOptions(TableCellsNames.CellData);
			ws.InsertDataTable(dataTable, options);
			//FormattingRangeGemBox.FormattingUsedRange(ws, TableCellsNames.CellData);
			const int numDec = 4; //номер колонки с датой решения
			DateTime maxData =
				dataTable.Rows.Cast<object>()
						 .Select((t, i) => Convert.ToDateTime(dataTable.Rows[i][numDec]))
						 .Max();
			int numMonth = maxData.Month;
			HeaderFile.Header = string.Format(@"Информация о предоставленных  рассрочках (отсрочках) по состоянию на {0:dd.MM.yyyy}",
							maxData);
			ws.Cells[HeaderFile.NameCellHeader].Value = HeaderFile.Header;//Заполняем ячейку с заголовком листа
			FormattingRangeGemBox.FormattingUsedRange(ws, TableCellsNames.CellData);//форматируем строки с данными

			//заполняем второй лист со списком платежей
			ws = workbook.Worksheets[2];
			options = new InsertDataTableOptions(TableCellsNames.CellData);
			dataTable = dataSet.Tables[1];
			ws.InsertDataTable(dataTable, options);
			FormattingRangeGemBox.FormattingUsedRange(ws, TableCellsNames.CellData);//форматируем строки с данными

			//сохраняем файл
			NewFile = string.Format(@"d:\Мои документы\Рассрочки\!Учет поступлений по рассрочке\рассрочки_2020_{0}.xlsx",
									numMonth);
			workbook.Save(NewFile);
		}
	}
}