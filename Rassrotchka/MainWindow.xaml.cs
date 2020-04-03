using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Rassrotchka.FilesCommon;
using Rassrotchka.NedoimkaDataSetTableAdapters;

namespace Rassrotchka
{
	/// <summary>
	///     Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private NedoimkaDataSet _dataSet;
		private DebitPayGenTableAdapter _debitPayGenTableAdapter;
		public MainWindow()
		{
			_dataSet = ((NedoimkaDataSet)(FindResource("nedoimkaDataSet")));
			InitializeComponent();
		}

		public ArgumentDebitPay Argument { get; set; }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_dataSet = ((NedoimkaDataSet) (FindResource("nedoimkaDataSet")));
			// Загрузить данные в таблицу DebitPayGen. Можно изменить этот код как требуется.
			_debitPayGenTableAdapter = new DebitPayGenTableAdapter();
			_debitPayGenTableAdapter.Fill(_dataSet.DebitPayGen);
			_dataSet.DebitPayGen.Rows[0].SetColumnError(7, @"Ошибка");//todo удалить
			var debitPayGenViewSource = ((CollectionViewSource) (FindResource("debitPayGenViewSource")));
			debitPayGenViewSource.View.MoveCurrentToFirst();
			// Загрузить данные в таблицу MonthPay. Можно изменить этот код как требуется.
			var nedoimkaDataSetMonthPayTableAdapter = new MonthPayTableAdapter();
			nedoimkaDataSetMonthPayTableAdapter.Fill(_dataSet.MonthPay);
			var debitPayGenMonthPayViewSource = ((CollectionViewSource) (FindResource("debitPayGenMonthPayViewSource")));
			debitPayGenMonthPayViewSource.View.MoveCurrentToFirst();
		}

		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			var arg = (ArgumentDebitPay) e.Argument;
			var fill = new FillTablesPayes1(arg);
			var tableDB = _dataSet.DebitPayGen;

			//var classCopyDbf = new ClassCopyDbfToSql(fileFullName, _sqlTableName, FilesNames.StartAllMobiFileDbf);
			//_args.DateType = classCopyDbf.Copy();
			//_countStr.RecodsCount = classCopyDbf.RecodsCount;
			//if (_sqlTableName == TablesNames.AllMobiTableNamesYur)//если обновление базы данных физ лиц
			//{
			//    try
			//    {
			//        var connection = new SqlConnection(Settings.Default.NedoimkaConnectionString);
			//        connection.Open();
			//        var command = connection.CreateCommand();
			//        command.CommandType = CommandType.StoredProcedure;
			//        command.CommandText = ProcFunctViewNames.ProcUpdNamPlat;
			//        _countStr.UpdatesCount = command.ExecuteNonQuery();
			//    }
			//    catch (Exception ex)
			//    {
			//        throw new Exception("Ошибка обновления таблицы Name_Plat" + ex.Message);
			//    }
			//}
			//e.Result = _countStr;
		}

		#region Обработчики событий иных элементов управления

		//обработка события перетаскивания файла
		private void grid_PreviewDragEnter(object sender, DragEventArgs e)
		{
			e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
		}

		private void Grid_PreviewDrop(object sender, DragEventArgs e)
		{
			//ButtonIsEnabled(false);
			var filePathArray = (string[]) e.Data.GetData(DataFormats.FileDrop, true);
			Argument.FilePath = filePathArray[0];
			Cursor = Cursors.Wait;
			var worker = new BackgroundWorker();
			worker.DoWork += worker_DoWork;
			//worker.RunWorkerCompleted += worker_RunWorkerCompleted;
			worker.RunWorkerAsync(Argument);
		}

		#endregion

		private void SaveCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			throw new System.NotImplementedException();
		}

		private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var dataTable = _dataSet.DebitPayGen.GetChanges();
			if (dataTable != null && dataTable.Rows.Count > 0)
			{
				e.CanExecute = false;
			}
			e.CanExecute = true;
		}
	}
}