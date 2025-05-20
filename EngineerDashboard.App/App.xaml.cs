using System.Windows;
using EngineerDashboard.App.Services;
using EngineerDashboard.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace EngineerDashboard.App;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        ServiceCollection services = new ServiceCollection();
        
        services.AddSingleton<TelemetryProvider>();

        services.AddSingleton<MainWindowViewModel>();
        
        services.AddSingleton<MainWindow>(sp =>
        {
            var vm = sp.GetRequiredService<MainWindowViewModel>();
            var window = new MainWindow
            {
                DataContext = vm
            };

            return window;
        });
        
        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        
        base.OnExit(e);
    }
}