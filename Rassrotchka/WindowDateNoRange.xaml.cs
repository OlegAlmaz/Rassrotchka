using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rassrotchka
{
	/// <summary>
	/// Логика взаимодействия для WindowDateNoRange.xaml
	/// Класс вспывающего окна, когда дата решения сомнительна вне реального интервала
	/// </summary>
	public partial class WindowDateNoRange : Window
	{
		public WindowDateNoRange()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
		}

		private void ButtonYes_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
