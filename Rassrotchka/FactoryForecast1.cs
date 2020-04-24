using System;
using System.Data;
using System.Data.SqlClient;
using Rassrotchka.FilesCommon;
using Rassrotchka.Properties;

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
		public override DataSet SqlToDataSet(ArgumentDebitPay args)
		{
			var sqlConnection = new SqlConnection(Settings.Default.NedoimkaConnectionString);
			using (var dataSet = new DataSet())
			{
				//заполнение сводной таблицы
				var sqlCommand = new SqlCommand(args.ProcedureName, sqlConnection)
				{
					CommandType = CommandType.StoredProcedure
				};
				var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
				sqlDataAdapter.SelectCommand.Parameters.Add("@DateFirst", SqlDbType.Date);
				sqlDataAdapter.SelectCommand.Parameters[0].SqlValue = args.DateFirst;
				sqlDataAdapter.SelectCommand.Parameters.Add("@DateEnd", SqlDbType.Date);
				sqlDataAdapter.SelectCommand.Parameters[0].SqlValue = args.DateEnd;
				sqlDataAdapter.Fill(dataSet);
				return dataSet;
			}
		}
	}

	public class FileForecast1 : AbstractFile
	{
		public override void Interact(object argument, AbstractTable tb)
		{
			throw new NotImplementedException();
		}
	}


}