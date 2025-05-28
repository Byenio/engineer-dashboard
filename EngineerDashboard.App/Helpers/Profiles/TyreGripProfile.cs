using System.Windows.Media;

namespace EngineerDashboard.App.Helpers.Profiles;

public class TyreGripProfile
{
    public Dictionary<byte, Brush> TemperatureToGrip { get; init; } = new();
}