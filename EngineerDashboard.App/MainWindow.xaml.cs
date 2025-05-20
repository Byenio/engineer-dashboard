using System.Windows;
using EngineerDashboard.App.Services;
using EngineerDashboard.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace EngineerDashboard.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(TelemetryService telemetryService)
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel(telemetryService);
    }
}