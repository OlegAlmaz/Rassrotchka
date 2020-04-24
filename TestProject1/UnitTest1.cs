using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
	[TestClass]
	public class UnitTest1
	{
		private DataTable GetDataTable()
		{
			int countRow = 5;
			var table = new DataTable("TableTest");
			var clm = new DataColumn("Id", Type.GetType("System.Int32"));
			table.Columns.Add(clm);
			clm = new DataColumn("Name", Type.GetType("System.String"));
			table.Columns.Add(clm);
			clm = new DataColumn("BirstDate", Type.GetType("System.DateTime"));
			table.Columns.Add(clm);
			clm = new DataColumn("Suma", Type.GetType("System.Decimal"));
			table.Columns.Add(clm);
			for (int i = 1; i <= countRow; i++)
			{
				var row = table.NewRow();
				row["Id"] = i;
				row["Name"] = "Ivanov_" + i;
				var date = new DateTime(1990, 1, 1);
				row["BirstDate"] = date.AddYears(i);
				row["Suma"] = 10000m + (i * 1000);
				table.Rows.Add(row);
			}
			return table;
		}
		/// <summary>
		/// Cоздает окно с тремя кнопками и Одним элементом TextBox для отображения информации
		/// </summary>
		/// <param name="text">Информация, которую необходимо вывести на экран в правой части</param>
		/// <param name="element"></param>
		/// <returns></returns>
		private static Window GetWindow(string text, UIElement element)
		{
			var window = new Window();
			window.Title = "Представление данных";
			window.FontSize = 14;

			var grid = new Grid { Name = "Grid1", Height = 100, Width = 250 };

			var converter = new GridLengthConverter();

			var row = new RowDefinition { Height = (GridLength)converter.ConvertFromString("*") };
			grid.RowDefinitions.Add(row);
			row = new RowDefinition { Height = (GridLength)converter.ConvertFromString("Auto") };
			grid.RowDefinitions.Add(row);

			var column = new ColumnDefinition { Width = (GridLength)converter.ConvertFromString("Auto") };
			grid.ColumnDefinitions.Add(column);
			column = new ColumnDefinition { Width = (GridLength)converter.ConvertFromString("Auto") };
			grid.ColumnDefinitions.Add(column);


			if (element != null)
			{
				Grid.SetColumn(element, 1);
				grid.Children.Add(element);
			}

			var textBox = new TextBox
				{
					Name = "TextBox1",
					TextWrapping = TextWrapping.Wrap,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					VerticalAlignment = VerticalAlignment.Stretch,
					TextAlignment = TextAlignment.Justify,
					Text = text
				};

			var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
			Grid.SetRow(stackPanel, 1);
			Grid.SetColumnSpan(stackPanel, 2);

			var button1 = new Button { Name = "ButonYes", Margin = new Thickness(5, 3, 20, 3), Content = "Да", IsDefault = true };
			var button2 = new Button { Name = "ButonNo", Margin = new Thickness(5, 3, 20, 3), Content = "Нет", IsCancel = true };
			var button3 = new Button { Name = "ButonCancel", Margin = new Thickness(5, 3, 20, 3), Content = "Отмена" };
			button1.Click += button1_Click;
			stackPanel.Children.Add(button1);
			stackPanel.Children.Add(button2);
			stackPanel.Children.Add(button3);

			grid.Children.Add(textBox);
			grid.Children.Add(stackPanel);

			window.Content = grid;
			return window;
		}

		static void button1_Click(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}

		[TestMethod]
		public void TestMethod1()
		{
			DataTable table = GetDataTable();

			//var dataGrid = new DataGrid();
			//dataGrid.SelectedIndex = 1;
			//var selectedItem = (DataRowView)dataGrid.SelectedItem;
			//var dataRow = selectedItem.Row;
			var builder = new StringBuilder();
			for (int i = 0; i < table.Rows.Count; i++)
			{
				for (int j = 0; j < table.Columns.Count; j++)
				{
					builder.Append(table.Rows[i][j]);
					builder.Append("\t");
				}
				if (i < table.Columns.Count - 1)
					builder.AppendLine();
			}
			var window = new ViewWindow();
			window.TextBox1.Text = builder.ToString();
			var result = (bool)window.ShowDialog();
			Assert.IsTrue((bool)result);

		}


	}

	public class ViewWindow : Window
	{
		private TextBox _textBox1;

		public TextBox TextBox1
		{
			get { return _textBox1; }
			set { _textBox1 = value; }
		}

		public ViewWindow()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			Title = "Представление данных";
			FontSize = 14;


			var grid = new Grid
				{
					Name = "Grid1",
					Height = 100,
					Width = 250,
					Background = new SolidColorBrush() {Color = Colors.Bisque}
				};

			var converter = new GridLengthConverter();

			var row = new RowDefinition { Height = (GridLength)converter.ConvertFromString("*") };
			grid.RowDefinitions.Add(row);
			row = new RowDefinition { Height = (GridLength)converter.ConvertFromString("Auto") };
			grid.RowDefinitions.Add(row);

			var column = new ColumnDefinition { Width = (GridLength)converter.ConvertFromString("Auto") };
			grid.ColumnDefinitions.Add(column);
			column = new ColumnDefinition { Width = (GridLength)converter.ConvertFromString("Auto") };
			grid.ColumnDefinitions.Add(column);

			_textBox1 = new TextBox
			{
				Name = "TextBox1",
				TextWrapping = TextWrapping.Wrap,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				TextAlignment = TextAlignment.Justify,
				Text = ""
			};

			var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
			Grid.SetRow(stackPanel, 1);
			Grid.SetColumnSpan(stackPanel, 2);

			var button1 = new Button { Name = "ButonYes", Margin = new Thickness(5, 3, 20, 3), Content = "Да", IsDefault = true };
			var button2 = new Button { Name = "ButonNo", Margin = new Thickness(5, 3, 20, 3), Content = "Нет", IsCancel = true };
			var button3 = new Button { Name = "ButonCancel", Margin = new Thickness(5, 3, 20, 3), Content = "Отмена" };
			button1.Click += button1_Click;
			stackPanel.Children.Add(button1);
			stackPanel.Children.Add(button2);
			stackPanel.Children.Add(button3);

			grid.Children.Add(_textBox1);
			grid.Children.Add(stackPanel);

			Content = grid;
			
		}

		public void SetUiElement(UIElement element)
		{
			Grid.SetColumn(element, 1);
			((Grid) this.Content).Children.Add(element);

		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}


	}

}
