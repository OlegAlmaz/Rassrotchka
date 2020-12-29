using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Rassrotchka.Converters
{
    public class CountRowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                int rowsCount = (int)value;
                if (!MainWindow.IsDirty)
                {
                    --rowsCount;
                }
                if (rowsCount == 0)
                {
                    return "";
                }
                return rowsCount;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
