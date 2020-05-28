using System.Data;
using System.Data.SqlClient;

namespace Rassrotchka
{
	/// <summary>
	/// формирует сводные данные о прогноозе поступлений по рассрочке на текущий месяц
	/// в разрезе плательщиков и платежей на 1 число
	/// </summary>
	public class FactoryForecast1 : AbstractFactory
	{
		public override AbstractTable CreateTable()
		{
			return new TableForecast1();
		}

		public override AbstractFile CreateFile()
		{
			return new FileForecast1();
		}
	}

	public class TableForecast1 : AbstractTable
	{

		protected override SqlDataAdapter SqlDataAdapter(SqlConnection sqlConnection)
		{
			var adapter = new SqlDataAdapter(BaseElementName.ProcedPredicGenPay, sqlConnection)
				{
					SelectCommand = {CommandType = CommandType.StoredProcedure}
				};
			adapter.SelectCommand.Parameters.Add("@DateFirst", SqlDbType.DateTime);
			adapter.SelectCommand.Parameters[0].SqlValue = Args.DateFirst;
			adapter.SelectCommand.Parameters.Add("@DateEnd", SqlDbType.DateTime);
			adapter.SelectCommand.Parameters[1].SqlValue = Args.DateEnd;
			return adapter;
		}

	}

	public class FileForecast1 : AbstractFile
	{
		public override void Interact(AbstractTable tb)
		{
			FilePath = "FilesTemplates\\Рассрочка_XX.xlsx";
			NewFile = string.Format(@"d:\Мои документы\Рассрочки\!Учет поступлений по рассрочке\Рассрочка_{0}.xlsx",
									tb.Args.Monthname.Imenit);
			HeaderFile.NameCellHeader = "A1";
			HeaderFile.Header = string.Format(@"Прогноз поступлений по рассроченным (отсроченным) суммам в {0} 2020 года",
										tb.Args.Monthname.Roditel);
			TableCellsNames.HeaderCollumns = "A2";
			TableCellsNames.CellData = "A3";

			DateToFile(tb);
		}
	}
}