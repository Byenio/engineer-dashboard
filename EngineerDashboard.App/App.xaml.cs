using System.Windows;
using EngineerDashboard.App.Services;
using EngineerDashboard.App.ViewModels;
using EngineerDashboard.App.Views;
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
        services.AddSingleton<SessionInfoViewModel>();
        services.AddSingleton<DriversRowViewModel>();
        services.AddSingleton<DriversTableViewModel>();
        
        services.AddSingleton<SessionInfoView>(sp => 
            new SessionInfoView { DataContext = sp.GetRequiredService<SessionInfoViewModel>() }
        );

        services.AddSingleton<DriversRowView>(sp => 
            new DriversRowView { DataContext = sp.GetRequiredService<DriversRowViewModel>() }
        );

        services.AddSingleton<DriversTableView>(sp => 
            new DriversTableView { DataContext = sp.GetRequiredService<DriversTableViewModel>() }
        );
        
        services.AddSingleton<MainWindow>();
        
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