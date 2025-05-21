using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Services;

namespace EngineerDashboard.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(TelemetryProvider telemetryProvider)
    {
    }
}