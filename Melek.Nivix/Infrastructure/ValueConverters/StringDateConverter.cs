using System;
using System.Globalization;
using System.Windows.Data;

namespace Nivix.Infrastructure.ValueConverters
{
    public class StringDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null) return ((DateTime)value).ToString("d");
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string typedValue = (value == null ? string.Empty : value.ToString());
            DateTime date;

            if (DateTime.TryParse(typedValue, out date)) { return date; }
            return null;
        }
    }
}
