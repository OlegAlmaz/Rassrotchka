using System;
using System.Data;
using System.Data.SqlClient;
using Rassrotchka.FilesCommon;
using Rassrotchka.Properties;

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
			throw new NotImplementedException();
		}
	}

	public class TableConsolid : AbstractTable
	{
		public override DataSet SqlToDataSet(ArgumentDebitPay args)
		{
			var sqlConnection = new SqlConnection(Settings.Default.NedoimkaConnectionString);
			using (var dataSet = new DataSet())
			{
				//заполнение сводной таблицы
				var sqlCommand = new SqlCommand(args.ProcedureName, sqlConnection) //todo указать название процедуры
					{
						CommandType = CommandType.StoredProcedure
					};
				var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
				sqlDataAdapter.SelectCommand.Parameters.Add("@Date", SqlDbType.Date);
				sqlDataAdapter.SelectCommand.Parameters[0].SqlValue = args.Date;
				sqlDataAdapter.Fill(dataSet);
				return dataSet;
			}
		}
	}

}