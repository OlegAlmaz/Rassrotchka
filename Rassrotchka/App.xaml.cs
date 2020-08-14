using System.Windows;
using StartupEventArgs = System.Windows.StartupEventArgs;

namespace Rassrotchka
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			Class1.NewMethod2(this);
			Class1.NewMethod1(this);
			Class1.NewMethod(this);
		}
	}
}
