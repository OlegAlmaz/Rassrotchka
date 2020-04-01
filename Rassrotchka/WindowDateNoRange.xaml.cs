using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Rassrotchka
{
	/// <summary>
	/// Логика взаимодействия для WindowDateNoRange.xaml
	/// Класс вспывающего окна, когда дата решения сомнительна вне реального интервала
	/// </summary>
	public partial class WindowDateNoRange : Window
	{
		public DataView  View { get; set; }
		public WindowDateNoRange()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (View != null) DataGrid1.ItemsSource = View;
		}

		private void ButtonYes_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void ButtonFlag_Click(object sender, RoutedEventArgs e)
		{
			var button = (Button) sender;
			button.Content = (string) button.Content == "Были" ? "Стали" : "Были";
			Label1.Content = (string)button.Content == "Были" ? "Стали" : "Были";
			View.RowStateFilter = (string) Label1.Content == "Были"
				                      ? DataViewRowState.ModifiedOriginal
				                      : DataViewRowState.ModifiedCurrent;
		}
	}
}
