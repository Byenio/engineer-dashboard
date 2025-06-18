using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EngineerDashboard.App.Helpers;

namespace EngineerDashboard.App.Converters;

public class PositionToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int position)
        {
            return ColorHelper.GetPositionBrush(position);
        }
        return Brushes.DimGray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}