using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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

		public void Run(object argument)
		{
			_file.Interact(argument, _table);
		}
	}

	public abstract class AbstractTable
	{
		public abstract DataSet SqlToDataSet(ArgumentDebitPay args);

		public DataSet FillDataSet(string sqlCommandText)
		{
			using (var sqlConnection = new SqlConnection(Settings.Default.NedoimkaConnectionString))
			{
				using (var dataSet = new DataSet())
				{
					var sqlDataAdapter = new SqlDataAdapter(sqlCommandText, sqlConnection);
					sqlDataAdapter.Fill(dataSet);
					return dataSet;
				}
			}
		}

	}

	public abstract class AbstractFile
	{
		//public FilterOPF FilterOpf;
		public abstract void Interact(object argument, AbstractTable tb);

	}

	public abstract class AbstractFactory
	{
		public abstract AbstractTable CreateTable();
		public abstract AbstractFile CreateFile();
	}

}
