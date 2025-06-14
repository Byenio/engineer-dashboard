using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EngineerDashboard.App.Services;
using EngineerDashboard.Database.Models;

namespace EngineerDashboard.App.ViewModels;

public partial class DriverInfoViewModel: ObservableObject
{
    private readonly DatabaseService _databaseService;
    
    [ObservableProperty] private string _driverName;
    [ObservableProperty] private int _driverId;
    [ObservableProperty] private int _elo;
    [ObservableProperty] private string _rankName;
    [ObservableProperty] private string _teamName;

    private ObservableCollection<RaceEntry> _races = new();

    public event Action GoBackRequested;

    public ObservableCollection<RaceEntry> Races
    {
        get => _races;
        set => SetProperty(ref _races, value);
    }

    public DriverInfoViewModel(DatabaseService databaseService, Driver driver)
    {
        _databaseService = databaseService;
        
        _driverId = driver.id;
        _driverName = driver.name;
        _elo = (int)driver.elo;
        _rankName = driver.rank?.name ?? "No rank";
        _teamName = driver.team?.name ?? "No Team";

        _ = LoadDriverRaces();
    }

    [RelayCommand]
    private void GoBack()
    {
        GoBackRequested.Invoke();
    }
    
    private async Task LoadDriverRaces()
    {
        var races = await _databaseService.GetRacesByDriver(_driverId);

        Races.Clear();

        foreach (var race in races)
        {
            Races.Add(race);
        }
    }
}