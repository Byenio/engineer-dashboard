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
        services.AddSingleton<TelemetryLoggerService>();
        
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<SessionInfoViewModel>();
        services.AddSingleton<DriversRowViewModel>();
        services.AddSingleton<DriversTableViewModel>();
        services.AddSingleton<TyreCardViewModel>();
        services.AddSingleton<LapTimeChartViewModel>();
        services.AddSingleton<TyreWearChartViewModel>();
        services.AddSingleton<FuelChartViewModel>();
        services.AddSingleton<BatteryChartViewModel>();
        services.AddSingleton<InputsChartViewModel>();
        services.AddSingleton<TelemetryChartViewModel>();
        services.AddSingleton<CarInfoCardViewModel>();
        
        services.AddSingleton<SessionInfoView>(sp => 
            new SessionInfoView { DataContext = sp.GetRequiredService<SessionInfoViewModel>() }
        );

        services.AddSingleton<DriversRowView>(sp => 
            new DriversRowView { DataContext = sp.GetRequiredService<DriversRowViewModel>() }
        );

        services.AddSingleton<DriversTableView>(sp => 
            new DriversTableView { DataContext = sp.GetRequiredService<DriversTableViewModel>() }
        );

        services.AddSingleton<TyreCardView>(sp => 
            new TyreCardView { DataContext = sp.GetRequiredService<TyreCardViewModel>() }
        );

        services.AddSingleton<LapTimeChartView>(sp =>
            new LapTimeChartView { DataContext = sp.GetRequiredService<LapTimeChartViewModel>() }
        );

        services.AddSingleton<TyreWearChartView>(sp =>
            new TyreWearChartView { DataContext = sp.GetRequiredService<TyreWearChartViewModel>() }
        );

        services.AddSingleton<FuelChartView>(sp =>
            new FuelChartView { DataContext = sp.GetRequiredService<FuelChartViewModel>() }
        );

        services.AddSingleton<BatteryChartView>(sp =>
            new BatteryChartView { DataContext = sp.GetRequiredService<BatteryChartViewModel>() }
        );

        services.AddSingleton<InputsChartView>(sp =>
            new InputsChartView { DataContext = sp.GetRequiredService<InputsChartViewModel>() }
        );

        services.AddSingleton<TelemetryChartView>(sp =>
            new TelemetryChartView { DataContext = sp.GetRequiredService<TelemetryChartViewModel>() }
        );

        services.AddSingleton<CarInfoCardView>(sp =>
            new CarInfoCardView { DataContext = sp.GetRequiredService<CarInfoCardViewModel>() }
        );
        
        services.AddSingleton<MainWindow>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        _ = _serviceProvider.GetRequiredService<TelemetryLoggerService>();
        
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        var mainViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        mainWindow.DataContext = mainViewModel;
        
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