using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class MainWindow
    {
        private bool _isDirty = true;
        private NedoimkaDataSet _dataSet;
        private readonly DebitPayGenTableAdapter _debitPayGenTableAdapter;
        private readonly MonthPayTableAdapter _monthPayTableAdapter;
        private CollectionViewSource _viewDpGn;
        private CollectionViewSource _viewMp;
        private ArgumentDebitPay _argument;
        private DataView _asDataView;

        public static UndoMenuItem<DataRow> UndoItem { get; private set; }

        /// <summary>
        /// элемент привязки BindingListCollectionView
        /// </summary>
        private BindingListCollectionView _view;

        public MainWindow()
        {
            UndoItem = new UndoMenuItem<DataRow>();
            InitializeComponent();
            _dataSet = ((NedoimkaDataSet)(FindResource("NedoimkaDataSet")));

            _debitPayGenTableAdapter = new DebitPayGenTableAdapter();
            _monthPayTableAdapter = new MonthPayTableAdapter();
        }

        private void DebitPaytable_RowChanged(object sender, DataRowChangeEventArgs e)
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
            _dataSet.DebitPayGen.RowChanged += DebitPaytable_RowChanged;
            _dataSet.MonthPay.RowChanged += DebitPaytable_RowChanged;

            _viewMp = ((CollectionViewSource)(FindResource("DebitPayGenMonthPayViewSource")));
            _viewDpGn = ((CollectionViewSource)(FindResource("DebitPayGenViewSource")));
            _view = CollectionViewSource.GetDefaultView(_viewDpGn.View) as BindingListCollectionView;
            _asDataView = _dataSet.DebitPayGen.DefaultView;

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
            _debitPayGenTableAdapter.Fill(_dataSet.DebitPayGen);
            //DebitPayGenDataGrid.ItemsSource = _viewDpGn.View;
            _viewDpGn.View.MoveCurrentToFirst();
            // Загрузить данные в таблицу MonthPay. Можно изменить этот код как требуется.
            _monthPayTableAdapter.Fill(_dataSet.MonthPay);

            AcceptChanges();
        }

        private void AcceptChanges()
        {
            _dataSet.DebitPayGen.AcceptChanges();
            _dataSet.MonthPay.AcceptChanges();
            UndoItem.Clear();
        }

        #region Обработчики событий иных элементов управления


        #region //обработка события перетаскивания файла

        private void Grid_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void Grid_PreviewDrop(object sender, DragEventArgs e)
        {
            LabelInfo.Content = string.Empty;
            var filePathArray = (string[])e.Data.GetData(DataFormats.FileDrop, true);
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
            _dataSet.DebitPayGen.RejectChanges();//todo переделать
            _dataSet.MonthPay.RejectChanges();//todo переделать
            UndoItem.Clear();
        }

        private void UpdateCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var tab1 = _dataSet.DebitPayGen.GetChanges();
            var tab2 = _dataSet.MonthPay.GetChanges();
            if (tab1 != null && tab2 != null)
            {
                var result = MessageBox.Show("Обновить базу данных?", "Информация.", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    _debitPayGenTableAdapter.Update(_dataSet.DebitPayGen);
                    _monthPayTableAdapter.Update(_dataSet.MonthPay);
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
                int count = _dataSet.DebitPayGen.Rows.Count;
                if (count == 0)
                    FillData();
                else
                    throw new Exception("База данных уже загружена!");
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

                    var payes = new FillTablesPayes1(_argument, _dataSet.DebitPayGen, _dataSet.MonthPay);
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
                        _asDataView.Sort = string.Format("{0} DESC", _dataSet.DebitPayGen.Date_DecisColumn.ColumnName);
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
            string namDeb = _dataSet.DebitPayGen.TableName;
            string namMont = _dataSet.MonthPay.TableName;
            if (_dataSet.HasChanges())
            {
                DataSet dsCnang = _dataSet.GetChanges();
                if (dsCnang.Tables[namDeb].Rows.Count > 0)
                {
                    DataRow[] dataRows = dsCnang.Tables[namDeb].GetErrors();
                    foreach (DataRow dataRow in dataRows)
                    {
                        dataRow.ClearErrors();
                        dataRow.RowError = string.Empty;
                    }
                    _debitPayGenTableAdapter.Update(_dataSet);
                }
                if (dsCnang.Tables[namMont].Rows.Count > 0)
                {
                    DataRow[] dataRows = dsCnang.Tables[namMont].GetErrors();
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
            if (_dataSet.DebitPayGen.Rows.Count == 0) //если не загружены таблицы загружаем
            {
                _debitPayGenTableAdapter.Fill(_dataSet.DebitPayGen);
                _monthPayTableAdapter.Fill(_dataSet.MonthPay);
                AcceptChanges();
            }
        }

        #endregion

        private void CloseCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_asDataView == null)
                return;

            var item = (MenuItem)sender;
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
                menuItem.IsChecked = ReferenceEquals(item, menuItem);
            }
        }

        private void TextBoxKod_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
            if (e.Key == Key.Enter)
            {
                _view.CustomFilter = ((TextBox)sender).Text.Length < 8
                         ? string.Empty
                         : string.Format("{0} = {1}", _dataSet.DebitPayGen.Kod_PayerColumn.ColumnName,
                                         ((TextBox)sender).Text);
            }
        }

        private void TextBoxGni_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Int16.TryParse(e.Text, out _))
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
                                         : string.Format("{0} = {1}", _dataSet.DebitPayGen.Kod_GNIColumn.ColumnName,
                                                         ((TextBox)sender).Text);
            }

        }

        private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _view.CustomFilter = ((TextBox)sender).Text.Length < 3
                                        ? string.Empty
                                        : string.Format("{0} LIKE \'%{1}%\'", _dataSet.DebitPayGen.NameColumn.ColumnName,
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

        private void MenuItemReport_OnClick(object sender, RoutedEventArgs e)
        {
            string nameMenu = ((MenuItem)sender).Name;
            int numFact = GetNumFact(nameMenu);//индекс фабрики
            LabelInfo.Content = "";
            Cursor = Cursors.Wait;
            var bw = new BackgroundWorker();
            bw.DoWork += BwOnDoWork;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.RunWorkerAsync(numFact);
        }

        private int GetNumFact(string nameMenu)
        {
            int numFact = 0;
            switch (nameMenu)
            {
                case "MenuItemConsolid"://сводная таблица на определенную дату
                    numFact = 0;
                    break;
                case "MenuItemForecast1"://прогноз поступлений на 1 число в разрезе плательщиков и платежей
                    SetPeiod();
                    numFact = 1;
                    break;
                case "MenuItemForecast20"://прогноз поступлений на 20 число в разрезе платежей
                    SetPeiod();
                    numFact = 2;
                    break;
            }
            return numFact;
        }

        private void SetPeiod()
        {
            var winInform = new Window1();
            var showDialog = winInform.ShowDialog();
            if (showDialog != null && (bool)showDialog)
            {
                if (winInform.DatePicker1.SelectedDate != null)
                    _argument.DateFirst = (DateTime)winInform.DatePicker1.SelectedDate;
                if (winInform.DatePicker2.SelectedDate != null)
                    _argument.DateEnd = (DateTime)winInform.DatePicker2.SelectedDate;

                var dict = new DictMonth();
                dict.TryGetValue(_argument.DateEnd.Month, out MonthName namesMonth);
                _argument.Monthname = namesMonth;
            }
        }

        private void BwOnDoWork(object sender, DoWorkEventArgs e)
        {
            var numFactory = (int)e.Argument;
            var factoriesList = new List<Lazy<AbstractFactory>>
                {
                    new Lazy<AbstractFactory>(() => new FactoryConsolid()),//0
					new Lazy<AbstractFactory>(() => new FactoryForecast1()),//1
					new Lazy<AbstractFactory>(() => new FactoryForecast20()),//2
	            };
            AbstractFactory abstractFactory = factoriesList[numFactory].Value;
            var client = new Client(abstractFactory);
            client.Run(_argument);
        }

        void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show(e.Error == null ? "Файл создан" : e.Error.Message);
            Cursor = null;
        }

        private void CloseWindow()
        {
            if (_dataSet.HasChanges())
                MessageBox.Show("Сохраните имеющиеся изменения данных перед закрытием программы!", "Предупреждение",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            else
                Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_dataSet.HasChanges())
            {
                e.Cancel = true;
                MessageBox.Show("Сохраните имеющиеся изменения данных перед закрытием программы!", "Предупреждение",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}