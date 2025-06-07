using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EngineerDashboard.App.Helpers;

namespace EngineerDashboard.App.Converters;

public class RankToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string rankName)
        {
            return ColorHelper.GetRankBrush(rankName);
        }
        return Brushes.DimGray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}