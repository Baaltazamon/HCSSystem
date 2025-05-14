using System.Globalization;
using System.Windows.Data;

namespace HCSSystem.Converters
{
    public class BoolToTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? "Владелец" : "Зарегистрирован";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
