using System.Windows.Media;
using EngineerDashboard.Telemetry;

namespace EngineerDashboard.App.Helpers;

public static class DrsColorHelper
{
    public static Brush GetBrush(bool drsAllowed, bool drsOpen)
    {
        if (drsOpen)
        {
            return Brushes.GreenYellow;
        }
        else if (drsAllowed)
        {
            return Brushes.DarkOliveGreen;
        }
        else
        {
            return Brushes.DimGray;
        }
    }
}