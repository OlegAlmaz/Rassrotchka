using System.Globalization;
using GemBox.Spreadsheet;

namespace Rassrotchka
{
	/// <summary>
	/// Осуществляет форматирование таблицы данных при испоьзовании
	/// бибилитеки GemBox
	/// </summary>
	public static class FormattingRangeGemBox
	{
		/// <summary>
		/// Форматирование ячеек таблицы данных на основании
		/// шаблона формата, становленного в 5 строке листа файла excel
		/// </summary>
		/// <param name="ws">лист книги</param>
		/// <param name="firstCell">первая ячейка таблицы, например "A2"</param>
		public static void FormattingUsedRange(ExcelWorksheet ws, string firstCell)
		{
			const int numRow = 5;//номер строки листа excel, с которой копируется стили ячеек
			CellRange range = ws.GetUsedCellRange(false);
			int count = range.LastColumnIndex + 1;
			char namEndClm = range.EndPosition[0];//название последней колонки таблицы
			var cellStyles = new CellStyle[count];
			var ncl = firstCell[0];

			//заполняем массив стилей ячеек отформатированной строки номер 5
			for (int i = 0; i < cellStyles.Length && ncl <= namEndClm; i++, ncl++)
			{
				string namCl = ncl + numRow.ToString(CultureInfo.InvariantCulture);
				cellStyles[i] = ws.Cells[namCl].Style;
			}

			//устанавливаем стили ячеек таблицы данных
			ExcelRowCollection rows = ws.Rows;
			foreach (ExcelRow row in rows)
			{
				if (row.Index > numRow)
				{
					int ind = 0;
					foreach (var cell in row.AllocatedCells)
					{
						cell.Style = cellStyles[ind];
						ind++;
					}
				}
			}
			//устанавливаем диапазон таблицы//todo уточнить параметры сдвига
			int num1 = 1;//сдвиг от начала по строкам
			int num2 = 0;//сдвиг от начала по колонкам
			var ragTable = range.GetSubrangeRelative(num1, num2, range.Width, range.Height - num1);
			//устанавливаем общую жирную рамку таблицы
			ragTable.Style.Borders.SetBorders(MultipleBorders.Outside, SpreadsheetColor.FromName(ColorName.Black), LineStyle.Medium);
		}
	}
}
