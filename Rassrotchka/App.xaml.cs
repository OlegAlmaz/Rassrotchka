using System;
using System.Windows;
using System.Windows.Navigation;

namespace Rassrotchka
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	public partial class App : Application
	{
        protected override void OnStartup(StartupEventArgs e)
        {
            App_OnStartup(new Action<Application>[]
            {
                app => Class1.NewMethod2(this)
                , app => Class1.NewMethod1(this)
                , app => Class1.NewMethod(this)
            }, this);
        }

        protected override void OnLoadCompleted(NavigationEventArgs e)
        {
            if ((bool) e.Content == false)
                Shutdown();
        }

        private void App_OnStartup(Action<Application>[] methods, Application app)
        {
            foreach (var m in methods)
                m(app);
        }
	}
}
