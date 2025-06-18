using System.Globalization;
using System.Windows.Data;
using EngineerDashboard.Telemetry;

namespace EngineerDashboard.App.Converters;

public class TyreCompoundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is VisualTyreCompound compound)
        {
            return compound switch
            {
                VisualTyreCompound.SOFT => "(S)",
                VisualTyreCompound.MEDIUM => "(M)",
                VisualTyreCompound.HARD => "(H)",
                VisualTyreCompound.INTER => "(I)",
                VisualTyreCompound.WET => "(W)",
                _ => string.Empty
            };
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}