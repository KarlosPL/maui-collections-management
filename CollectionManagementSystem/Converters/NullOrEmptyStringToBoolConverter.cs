using System.Globalization;

namespace CollectionManagementSystem.Converters
{
    public class NullOrEmptyStringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string stringValue && !string.IsNullOrEmpty(stringValue) && stringValue != "empty" 
                || value is int intValue && intValue != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
