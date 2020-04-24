using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using Rassrotchka.FilesCommon;
using Rassrotchka.NedoimkaDataSetTableAdapters;

namespace Rassrotchka
{
	/// <summary>
	///     Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private bool _isDirty = true;
		private NedoimkaDataSet _dataSet;
		private NedoimkaDataSet.DebitPayGenDataTable _debitPaytable;
		private NedoimkaDataSet.MonthPayDataTable _monthPayTable;
		private DebitPayGenTableAdapter _debitPayGenTableAdapter;
		private MonthPayTableAdapter _monthPayTableAdapter;
		private CollectionViewSource _viewDpGn;
		private CollectionViewSource _viewMp;
		private ArgumentDebitPay _argument;
		private DataView _asDataView;

		public static UndoMenuItem<DataRow> UndoItem { get; private set; }
		private BindingListCollectionView _view;

		public MainWindow()
		{
			UndoItem = new UndoMenuItem<DataRow>();
			InitializeComponent();
			_dataSet = ((NedoimkaDataSet)(FindResource("NedoimkaDataSet")));


			_debitPaytable = _dataSet.DebitPayGen;
			_monthPayTable = _dataSet.MonthPay;

			_debitPayGenTableAdapter = new DebitPayGenTableAdapter();
			_monthPayTableAdapter = new MonthPayTableAdapter();
		}

		void _debitPaytable_RowChanged(object sender, DataRowChangeEventArgs e)
		{
			if (e.Row.RowState != DataRowState.Unchanged && e.Row.RowState != DataRowState.Detached)
			{
				UndoItem.Add(e.Row);
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_argument = new ArgumentDebitPay();

			_dataSet = ((NedoimkaDataSet)(FindResource("NedoimkaDataSet")));
			_debitPaytable.RowChanged += _debitPaytable_RowChanged;
			_monthPayTable.RowChanged += _debitPaytable_RowChanged;

			_viewMp = ((CollectionViewSource)(FindResource("DebitPayGenMonthPayViewSource")));
			_viewDpGn = ((CollectionViewSource)(FindResource("DebitPayGenViewSource")));
			_view = CollectionViewSource.GetDefaultView(_viewDpGn.View) as BindingListCollectionView;
			_asDataView = _debitPaytable.DefaultView;

			var binding = new Binding("IsEnabled")
				{
					Source = UndoItem,
					Mode = BindingMode.OneWay,
					UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
				};
			MenuItemContextUndo.SetBinding(IsEnabledProperty, binding);
		}

		private void FillData()
		{

			// Загрузить данные в таблицу DebitPayGen. Можно изменить этот код как требуется.
			_debitPayGenTableAdapter.Fill(_debitPaytable);
			//DebitPayGenDataGrid.ItemsSource = _viewDpGn.View;
			_viewDpGn.View.MoveCurrentToFirst();
			// Загрузить данные в таблицу MonthPay. Можно изменить этот код как требуется.
			_monthPayTableAdapter.Fill(_monthPayTable);

			AcceptChanges();
		}

		private void AcceptChanges()
		{
			_debitPaytable.AcceptChanges();
			_monthPayTable.AcceptChanges();
			UndoItem.Clear();
		}

		#region Обработчики событий иных элементов управления



		#region //обработка события перетаскивания файла

		private void grid_PreviewDragEnter(object sender, DragEventArgs e)
		{
			e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
		}

		private void Grid_PreviewDrop(object sender, DragEventArgs e)
		{
			LabelInfo.Content = string.Empty;
			var filePathArray = (string[]) e.Data.GetData(DataFormats.FileDrop, true);
			_argument.FilePath = filePathArray[0];
			LabelInfo.Content = Path.GetFileName(_argument.FilePath);
			AddNewData();
		}

		#endregion

		#endregion

		#region Обработка комманд

		private void NotReadonly()
		{
			_isDirty = false;

			DebitPayGenDataGrid.IsReadOnly = false;
			MonthPayDataGrid.IsReadOnly = false;
		}

		private void Readonly()
		{
			_isDirty = true;
			DebitPayGenDataGrid.IsReadOnly = true;
			MonthPayDataGrid.IsReadOnly = true;
		}

		private void SaveCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			Readonly();
			ValidFromCanges();//Проверка на наличие изменений в таблице, сохранение и обновление базые данных
		}

		private void SaveCommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			
			e.CanExecute = !_isDirty;
		
		}

		private void EditCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			NotReadonly();
		}

		private void EditCommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = _isDirty;
		}

		private void UndoCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			int i = UndoItem.Count - 1;
			UndoItem.List[i].RejectChanges();
			UndoItem.List[i].RowError = string.Empty;
			UndoItem.List[i].ClearErrors();
			UndoItem.RemoveAt(i);
				
		}

		private void UndoAllCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			_debitPaytable.RejectChanges();//todo переделать
			_monthPayTable.RejectChanges();//todo переделать
			UndoItem.Clear();
		}

		private void UpdateCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var tab1 = _debitPaytable.GetChanges();
			var tab2 = _monthPayTable.GetChanges();
			if (tab1 != null && tab2 != null)
			{
				var result = MessageBox.Show("Обновить базу данных?", "Информация.", MessageBoxButton.YesNo, MessageBoxImage.Information);
				if (result == MessageBoxResult.Yes)
				{
					_debitPayGenTableAdapter.Update(_debitPaytable);
					_monthPayTableAdapter.Update(_monthPayTable);
					AcceptChanges();

					_isDirty = true;
				}
			}
		}

		private void FillDataCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			Cursor = Cursors.Wait;
			try
			{
				FillData();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			Cursor = null;
		}

		#region Загрузка новых данных в базу

		private void DownloadCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			LabelInfo.Content = string.Empty;
			var dialog = new OpenFileDialog
				{
					Title = "Выбор файлов",
					Multiselect = false,
					InitialDirectory = @"d:\Мои документы\Рассрочки\!Учет поступлений по рассрочке\Контроль"
				};
			var result = dialog.ShowDialog();
			if (result == true)
			{
				_argument.FilePath = dialog.FileName;
				LabelInfo.Content = dialog.SafeFileName;
				AddNewData();
			}
		}

		private void AddNewData()
		{
			if (_isDirty == false)
			{
				MessageBox.Show("Нажмите на кнопку сохранить, перед внесением новых данных в таблицу");
				return;
			}
			if (_argument.FilePath.EndsWith(".xls") || _argument.FilePath.EndsWith(".xlsx"))
			{
				Cursor = Cursors.Wait;
				try
				{

					ValidateFromLoad(); //Проверка: загружен ли dataset, если нет, то загружаем
					ValidFromCanges(); //Проверка: есть ли измененные строки в dataset, если да, то сохраняем и обновляем базу данных

					var payes = new FillTablesPayes1(_argument, _debitPaytable, _monthPayTable);
					payes.UpdateSqlTableDebitPayGen();

					_asDataView.RowStateFilter = DataViewRowState.ModifiedCurrent;
					int countModifRows = _asDataView.Count;
					_asDataView.RowStateFilter = DataViewRowState.Added;
					int countAddedRows = _asDataView.Count;

					if (countAddedRows + countModifRows == 0)
					{
						_asDataView.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
						MessageBox.Show("Новые и измененные данные отсутствуют");
					}
					else
					{
						MessageBox.Show("Добавлены и изменены следующие данные");
						_asDataView.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
						_asDataView.Sort = string.Format("{0} DESC", _debitPaytable.Date_DecisColumn.ColumnName);
						ItemAddAndModif.IsChecked = true;
						NotReadonly();
						_viewDpGn.View.MoveCurrentToFirst();
						_viewMp.View.MoveCurrentToFirst();
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
			else
			{
				MessageBox.Show("Неверное расширение файла: " + LabelInfo.Content);
			}
			Cursor = null;
		}

		private void ValidFromCanges()
		{
			if (_dataSet.HasChanges())
			{
				DataSet dsCnang = _dataSet.GetChanges();
				if (dsCnang.Tables[_debitPaytable.TableName].Rows.Count > 0)
				{
					DataRow[] dataRows = dsCnang.Tables[_debitPaytable.TableName].GetErrors();
					foreach (DataRow dataRow in dataRows)
					{
						dataRow.ClearErrors();
						dataRow.RowError = string.Empty;
					}
					_debitPayGenTableAdapter.Update(_dataSet);
				}
				if (dsCnang.Tables[_monthPayTable.TableName].Rows.Count > 0)
				{
					DataRow[] dataRows = dsCnang.Tables[_monthPayTable.TableName].GetErrors();
					foreach (DataRow dataRow in dataRows)
					{
						dataRow.ClearErrors();
						dataRow.RowError = string.Empty;
					}
					_monthPayTableAdapter.Update(_dataSet);
				}
				AcceptChanges();
			}
		}

		private void ValidateFromLoad()
		{
			if (_debitPaytable.Rows.Count == 0) //если не загружены таблицы загружаем
			{
				_debitPayGenTableAdapter.Fill(_debitPaytable);
				_monthPayTableAdapter.Fill(_monthPayTable);
				AcceptChanges();
			}
		}

		#endregion


		private void CleanCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			int rowCahng = _debitPaytable.GetChanges().Rows.Count;
			if (rowCahng > 0)
			{
				var result = MessageBox.Show("В таблице имеются измененные данные. \rОбновить базу данных перед очисткой экрана?",
				                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
				if (result == MessageBoxResult.Yes)
					_debitPayGenTableAdapter.Update(_dataSet);

			}
			DataRelationCollection relationCollection = _debitPaytable.ChildRelations;
			_dataSet.Clear();
		}


		#endregion

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (_asDataView == null)
				return;

			var item = (MenuItem) sender;
			string name = item.Name;
			switch (name)
			{
				case "ItemAll":
					_asDataView.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent | DataViewRowState.Unchanged;
					break;
				case "ItemAddAndModif":
					_asDataView.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
					break;
				case "ItemAdd":
					_asDataView.RowStateFilter = DataViewRowState.Added;
					break;
				case "ItemDel":
					_asDataView.RowStateFilter = DataViewRowState.Deleted;
					break;
				case "ItemCur":
					_asDataView.RowStateFilter = DataViewRowState.ModifiedCurrent;
					break;
				case "ItemOrig":
					_asDataView.RowStateFilter = DataViewRowState.ModifiedOriginal;
					break;
				case "ItemUnchang":
					_asDataView.RowStateFilter = DataViewRowState.Unchanged;
					break;
			}
		}

		private void Item_OnChecked(object sender, RoutedEventArgs e)
		{
			var list = new List<MenuItem>
				{
					ItemAll,
					ItemAddAndModif,
					ItemAdd,
					ItemDel,
					ItemCur,
					ItemOrig,
					ItemUnchang
				};

			var item = (MenuItem)sender;
			foreach (MenuItem menuItem in list)
			{
				if (ReferenceEquals(item, menuItem))
					menuItem.IsChecked = true;
				else
					menuItem.IsChecked = false;
			}
		}

		//Фильтр по коду ГНИ
		private void TextBoxGni_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			_view.CustomFilter = ((TextBox)sender).Text == string.Empty
									 ? string.Empty
									 : string.Format("{0} = {1}", _debitPaytable.Kod_GNIColumn.ColumnName,
													 ((TextBox)sender).Text);
		}

		//Фильтр по коду предприятия
		private void TextBoxKod_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			//_view.CustomFilter = ((TextBox) sender).Text.Length < 8
			//                         ? string.Empty
			//                         : string.Format("{0} = {1}", _debitPaytable.Kod_PayerColumn.ColumnName,
			//                                         ((TextBox) sender).Text);

		}

		private void TextBoxKod_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
				e.Handled = true;
			if (e.Key == Key.Enter)
			{
				_view.CustomFilter = ((TextBox)sender).Text.Length < 8
						 ? string.Empty
						 : string.Format("{0} = {1}", _debitPaytable.Kod_PayerColumn.ColumnName,
										 ((TextBox)sender).Text);
			}
		}

		private void TextBoxGni_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			short val;
			if (!Int16.TryParse(e.Text, out val))
				e.Handled = true;
		}

		private void TextBoxGni_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
				e.Handled = true;
			if (e.Key == Key.Enter)
			{
				_view.CustomFilter = ((TextBox)sender).Text == string.Empty
										 ? string.Empty
										 : string.Format("{0} = {1}", _debitPaytable.Kod_GNIColumn.ColumnName,
														 ((TextBox)sender).Text);
			}

		}

		private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
		{
			_view.CustomFilter = ((TextBox)sender).Text.Length < 3
										? string.Empty
										: string.Format("{0} LIKE \'%{1}%\'", _debitPaytable.NameColumn.ColumnName,
														((TextBox)sender).Text);
		}

		private void UndoSelectedRowsMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (DebitPayGenDataGrid.SelectedItem != null)
			{
				IList selectedItems = DebitPayGenDataGrid.SelectedItems;
				List<DataRow> rows = (from object selectedItem in selectedItems select ((DataRowView)selectedItem).Row).ToList();
				foreach (var row in rows)
				{
					row.RejectChanges();
					row.ClearErrors();
					row.RowError = "";
					UndoItem.Remote(row);
				}
			}
		}
	}
}