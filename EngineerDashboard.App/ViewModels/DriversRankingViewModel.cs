using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Services;
using EngineerDashboard.Database.Models;

namespace EngineerDashboard.App.ViewModels;

public class DriversRankingViewModel : ObservableObject, IDisposable
{
    private readonly DatabaseService _databaseService;
    private ObservableCollection<Driver> _drivers;

    public ObservableCollection<Driver> Drivers
    {
        get => _drivers;
        set => SetProperty(ref _drivers, value);
    }

    public DriversRankingViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        _drivers = new ObservableCollection<Driver>();

        LoadDriversAsync();
    }

    private async void LoadDriversAsync()
    {
        var drivers = await _databaseService.GetDrivers();
        Drivers = new ObservableCollection<Driver>(drivers);
    }

    public void Dispose()
    {
        _databaseService.Dispose();
    }
}