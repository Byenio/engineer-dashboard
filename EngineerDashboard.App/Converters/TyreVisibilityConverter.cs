using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EngineerDashboard.App.Converters;

public class TyreVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        bool available = values[0] is bool && (bool)values[0];
        bool isOnCar = values[1] is bool && (bool)values[1];

        return (available && !isOnCar) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}