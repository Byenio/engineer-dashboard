using System.Windows.Media;
using EngineerDashboard.Telemetry;

namespace EngineerDashboard.App.Helpers;

public static class DrsColorHelper
{
    public static Brush GetBrush(byte drsAllowed, byte drsOpen)
    {
        if (drsOpen == 1)
        {
            return Brushes.GreenYellow;
        }
        if (drsAllowed == 1)
        {
            return Brushes.DarkOliveGreen;
        }
        
        return Brushes.DimGray;
    }
}