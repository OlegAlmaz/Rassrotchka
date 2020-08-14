using System.Windows;

namespace Rassrotchka
{
    /// <summary>
    /// Логика взаимодействия для WindowValidSumPayers.xaml
    /// </summary>
    public partial class WindowValidSumPayers : Window
    {
        private NedoimkaDataSet _nedoimkaDataSet;
        public WindowValidSumPayers()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void Load()
        {
            _nedoimkaDataSet = ((Rassrotchka.NedoimkaDataSet)(FindResource("nedoimkaDataSet")));
            // TODO: Добавить сюда код, чтобы загрузить данные в таблицу ProcedureValidSumPayers.
            // Не удалось создать этот код, поскольку метод nedoimkaDataSetProcedureValidSumPayersTableAdapter.Fill отсутствует или имеет неизвестные параметры.
            var adapter = new Rassrotchka.NedoimkaDataSetTableAdapters.ProcedureValidSumPayersTableAdapter();
            adapter.Fill(_nedoimkaDataSet.ProcedureValidSumPayers);
            var procedureValidSumPayersViewSource = ((System.Windows.Data.CollectionViewSource)(FindResource("procedureValidSumPayersViewSource")));
            procedureValidSumPayersViewSource.View.MoveCurrentToFirst();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            _nedoimkaDataSet = new NedoimkaDataSet();
            Load();
        }
    }
}
