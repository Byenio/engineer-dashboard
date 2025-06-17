using System.Collections.ObjectModel;
using System.Diagnostics;
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
    [ObservableProperty] private Rank _rank;
    [ObservableProperty] private string _teamName;
    [ObservableProperty] private int _careerRaces;
    [ObservableProperty] private int _careerWins;
    [ObservableProperty] private int _careerPodiums;
    [ObservableProperty] private int _careerTop10s;
    [ObservableProperty] private int _careerPoints;

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
        _rank = driver.rank;
        _teamName = driver.team?.name ?? "No Team";

        _ = LoadDriverStats();
    }

    [RelayCommand]
    private void GoBack()
    {
        GoBackRequested.Invoke();
    }
    
    private async Task LoadDriverStats()
    {
        CareerRaces = await _databaseService.GetDriverRacesCount(DriverId);
        CareerWins = await _databaseService.GetDriverWinsCount(DriverId);
        CareerPodiums = await _databaseService.GetDriverTopFinishesCount(DriverId, 3);
        CareerTop10s = await _databaseService.GetDriverTopFinishesCount(DriverId, 10);
        CareerPoints = await _databaseService.GetDriverPointsCount(DriverId);
        
        var races = await _databaseService.GetRacesByDriver(DriverId);

        Races.Clear();

        foreach (var race in races)
        {
            Races.Add(race);
        }
    }
}