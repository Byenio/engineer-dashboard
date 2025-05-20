using System.Windows;
using EngineerDashboard.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EngineerDashboard.App;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        ServiceCollection services = new ServiceCollection();
        
        services.AddSingleton<TelemetryService>();
        
        services.AddTransient<MainWindow>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        MainWindow = mainWindow;
        
        MainWindow.Show();
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