using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EngineerDashboard.App.Helpers;
using EngineerDashboard.Telemetry;

namespace EngineerDashboard.App.Converters;

public class TeamToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string team)
        {
            return ColorHelper.GetTeamBrush(team.ToUpper());
        }
        return Brushes.DimGray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}