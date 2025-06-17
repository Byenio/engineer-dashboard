using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EngineerDashboard.App.Helpers;
using EngineerDashboard.App.ViewModels;
using EngineerDashboard.Telemetry;

namespace EngineerDashboard.App.Converters;

public class TyreToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is VisualTyreCompound tyre)
        {
            return ColorHelper.GetTyreBrush(tyre);
        }
        return Brushes.DimGray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}