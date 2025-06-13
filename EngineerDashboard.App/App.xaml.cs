using System.Diagnostics;
using System.Windows;
using EngineerDashboard.App.Services;
using EngineerDashboard.App.ViewModels;
using EngineerDashboard.App.Views;
using EngineerDashboard.Database;
using EngineerDashboard.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EngineerDashboard.App;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        DotNetEnv.Env.TraversePath().Load();
        
        ServiceCollection services = new ServiceCollection();
        
        services.AddSingleton<TelemetryProvider>();
        services.AddSingleton<TelemetryLoggerService>();
        
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = $"Host={Environment.GetEnvironmentVariable("DATABASE_ADDRESS")};" +
                                   $"Port={Environment.GetEnvironmentVariable("DATABASE_PORT")};" +
                                   $"Database={Environment.GetEnvironmentVariable("DATABASE_NAME")};" +
                                   $"Username={Environment.GetEnvironmentVariable("DATABASE_USER")};" +
                                   $"Password={Environment.GetEnvironmentVariable("DATABASE_PASSWORD")};";
            
            options.UseNpgsql(connectionString);
        }, ServiceLifetime.Singleton);
        services.AddSingleton<DatabaseService>();
        
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
        services.AddSingleton<DamageCardViewModel>();
        services.AddSingleton<DriversRankingViewModel>();
        services.AddSingleton<DriverInfoViewModel>();

        services.AddSingleton<Driver>();
        
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

        services.AddSingleton<DamageCardView>(sp =>
            new DamageCardView { DataContext = sp.GetRequiredService<DamageCardViewModel>() }
        );

        services.AddSingleton<DriversRankingView>(sp =>
            new DriversRankingView { DataContext = sp.GetRequiredService<DriversRankingViewModel>() }
        );

        services.AddSingleton<DriverInfoView>(sp =>
            new DriverInfoView { DataContext = sp.GetRequiredService<DriverInfoViewModel>() }
        );
        
        services.AddSingleton<MainWindow>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        _ = _serviceProvider.GetRequiredService<TelemetryLoggerService>();
        
        _ = _serviceProvider.GetRequiredService<DatabaseService>();
        
        var dbContext = _serviceProvider.GetRequiredService<AppDbContext>();

        if (!dbContext.Database.CanConnect())
        {
            MessageBox.Show(
                "Database connection could not be established.",
                "Database connection failure",
                MessageBoxButton.OK,
                MessageBoxImage.Error
                );
            Shutdown();
        }
        
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