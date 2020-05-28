using System.Data;
using System.Data.SqlClient;

namespace Rassrotchka
{
	/// <summary>
	/// формирует сводные данные о прогноозе поступлений по рассрочке на текущий месяц
	/// в разрезе плательщиков и платежей на 1 число
	/// </summary>
	public class FactoryForecast20 : AbstractFactory
	{
		public override AbstractTable CreateTable()
		{
			return new TableForecast20();
		}

		public override AbstractFile CreateFile()
		{
			return new FileForecast20();
		}
	}

	public class TableForecast20 : AbstractTable
	{

		protected override SqlDataAdapter SqlDataAdapter(SqlConnection sqlConnection)
		{
			var adapter = new SqlDataAdapter(BaseElementName.ProcedGeNPayFromPlatej, sqlConnection)
			{
				SelectCommand = { CommandType = CommandType.StoredProcedure }
			};
			adapter.SelectCommand.Parameters.Add("@DateStart", SqlDbType.DateTime);
			adapter.SelectCommand.Parameters[0].SqlValue = Args.DateFirst;
			adapter.SelectCommand.Parameters.Add("@DateEnd", SqlDbType.DateTime);
			adapter.SelectCommand.Parameters[1].SqlValue = Args.DateEnd;
			return adapter;
		}

	}

	public class FileForecast20 : AbstractFile
	{

		public override void Interact(AbstractTable tb)
		{
			FilePath = "FilesTemplates\\Рассрочки_налоги.xlsx";
			NewFile = string.Format(@"d:\Мои документы\Рассрочки\!Учет поступлений по рассрочке\Рассрочка_налоги_{0}.xlsx",
									tb.Args.Monthname.Imenit);
			HeaderFile.NameCellHeader = "A1";
			HeaderFile.Header = string.Format(@"Прогноз поступлений за счет погашения рассроченных и отсроченых сумм в {0} 2020 года",
										tb.Args.Monthname.Roditel);
			TableCellsNames.HeaderCollumns = "A2";
			TableCellsNames.CellData = "A3";

			DateToFile(tb);
		}
	}
}