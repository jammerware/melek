using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Nivix.Infrastructure.ValueConverters
{
    public class ArrayStringConverter : IValueConverter
    {
        public string Delimiter { get; set; }

        public ArrayStringConverter()
        {
            Delimiter = Environment.NewLine;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null) {
                string[] typedVal = (string[])value;
                StringBuilder builder = new StringBuilder();

                foreach (string item in typedVal) {
                    builder.Append(item);
                    builder.Append(Delimiter);
                }

                return builder.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}