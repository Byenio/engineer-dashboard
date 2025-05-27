using System.Windows.Media;
using EngineerDashboard.Telemetry;

namespace EngineerDashboard.App.Helpers;

public static class TeamColorHelper
{
    public static Brush GetBrush(Team team) => team switch
    {
        Team.MERCEDES => Brushes.Aqua,
        Team.FERRARI => Brushes.Red,
        Team.REDBULL => Brushes.MidnightBlue,
        Team.WILLIAMS => Brushes.DodgerBlue,
        Team.ASTONMARTIN => Brushes.DarkGreen,
        Team.ALPINE => Brushes.RoyalBlue,
        Team.ALPHATAURI => Brushes.SteelBlue,
        Team.HAAS => Brushes.DimGray,
        Team.MCLAREN => Brushes.DarkOrange,
        Team.ALFAROMEO => Brushes.DarkRed,
        Team.F1WORLD => Brushes.Goldenrod,
        _ => Brushes.Indigo
    };
}