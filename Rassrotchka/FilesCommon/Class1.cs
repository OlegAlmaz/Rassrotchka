using System;
using System.IO;
using System.Security.Principal;
using System.Windows;

namespace Rassrotchka
{
    static class Class1
    {
        public static void NewMethod2(App app)
        {
            NewMethod2_1(app);
        }

        private static void NewMethod2_1(App app)
        {
            const string fullFileName = @"c:\Users\d19-Osheiko\Desktop\TextFile1.txt";
            string numberString = "";
            if (File.Exists(fullFileName))
            {
                numberString = File.ReadAllText(fullFileName);
            }

            var ap = AppDomain.CurrentDomain;
            ap.SetData("help", true);
            int.TryParse(numberString, out int number);
            if (number != 5)
                app.Shutdown();
        }

        public static void NewMethod1(App app)
        {
            NewMethod1_1(app);
        }

        private static void NewMethod1_1(App app)
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null && !string.Equals(windowsIdentity.Name, "LG\\D19-OSHEIKO", StringComparison.OrdinalIgnoreCase))
                app.Shutdown();
        }

        public static void NewMethod(App app)
        {
            NewMethod_1(app);
        }

        private static void NewMethod_1(App app)
        {
            const string fileName = @"c:\Users\d19-Osheiko\textFile.txt";
            var fileInfo = new FileInfo(fileName);
            if (!File.Exists(fileName))
            {
                using (StreamWriter writer = fileInfo.CreateText())
                {
                    writer.WriteLine(DateTime.Now.ToShortDateString());
                }
                fileInfo.Attributes = FileAttributes.Hidden;
            }
            string datesString = File.ReadAllText(fileName);
            if (string.IsNullOrEmpty(datesString))
                datesString = DateTime.Now.ToShortDateString();
            var date = DateTime.Parse(datesString);
            var dateEnd = new DateTime(2021, 06, 24);
            if (date > dateEnd)
                MessageBox.Show("Вы правильно выставили системную дату на компьютере?");
            if (DateTime.Now >= dateEnd || DateTime.Now < date)
                app.Shutdown();
            else
            {
                if (!File.Exists(fileName))
                {
                    fileInfo.Create();
                    fileInfo.Attributes = FileAttributes.Hidden;
                }
                fileInfo.Attributes = FileAttributes.Normal;
                File.WriteAllText(fileName, DateTime.Now.ToShortDateString());
                fileInfo.Attributes = FileAttributes.Hidden;
            }
        }
    }
}
