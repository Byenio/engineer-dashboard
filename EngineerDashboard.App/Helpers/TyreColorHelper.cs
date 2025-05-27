using System.Windows.Media;
using EngineerDashboard.Telemetry;

namespace EngineerDashboard.App.Helpers;

public static class TyreColorHelper
{
    public static Brush GetBrush(VisualTyreCompound tyreCompound) => tyreCompound switch
    {
        VisualTyreCompound.SOFT => Brushes.Red,
        VisualTyreCompound.MEDIUM => Brushes.Gold,
        VisualTyreCompound.HARD => Brushes.White,
        VisualTyreCompound.INTER => Brushes.ForestGreen,
        VisualTyreCompound.WET => Brushes.Blue,
        _ => Brushes.Black
    };
}