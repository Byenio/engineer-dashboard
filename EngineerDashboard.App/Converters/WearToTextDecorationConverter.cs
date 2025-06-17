using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EngineerDashboard.App.Converters;

public class WearToTextDecorationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte wear)
        {
            return wear != 0 ? TextDecorations.Strikethrough : null;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}