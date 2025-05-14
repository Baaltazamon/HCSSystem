using System.Globalization;
using System.Windows.Data;

namespace HCSSystem.Converters
{
    public class ActiveToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            (bool)value ? "AccountLock" : "AccountCheck";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

}
