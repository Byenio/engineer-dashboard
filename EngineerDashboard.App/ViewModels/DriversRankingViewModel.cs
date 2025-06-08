using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Services;
using EngineerDashboard.Database.Models;

namespace EngineerDashboard.App.ViewModels;

public class DriversRankingViewModel : ObservableObject, IDisposable
{
    private readonly DatabaseService _databaseService;
    private ObservableCollection<Driver> _drivers = new();

    public ObservableCollection<Driver> Drivers
    {
        get => _drivers;
        set => SetProperty(ref _drivers, value);
    }

    public DriversRankingViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;

        _ = LoadDriversAsync();
    }

    public async Task LoadDriversAsync()
    {
        var drivers = await _databaseService.GetDrivers();
        var sortedDrivers = drivers.OrderByDescending(d => d.elo).ToList();
        
        Drivers.Clear();
        foreach (var driver in sortedDrivers)
        {
            Drivers.Add(driver);
        }
    }

    public void Dispose()
    {
        _databaseService.Dispose();
    }
}