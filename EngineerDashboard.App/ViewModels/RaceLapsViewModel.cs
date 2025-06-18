using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EngineerDashboard.App.Helpers;
using EngineerDashboard.App.Services;
using EngineerDashboard.Database.Models;

namespace EngineerDashboard.App.ViewModels;

public class LapDisplayModel
{
    public Lap Lap { get; set; }
    public int PitStopTimeMs { get; set; }

    public string DeltaFront => Formatter.FormatMsToDeltaString((short)Lap.delta_front);
    public string DeltaLeader => Formatter.FormatMsToDeltaString((short)Lap.delta_leader);
    public string TyreCompound => $"({Lap.tyre_compound.name[..1]})";
    public Brush TyreBrush => ColorHelper.GetTyreBrush(Lap.tyre_compound.name);
    public string TyreWear => $"{Lap.tyre_wear}%";
    public string LastLapTime => Formatter.FormatMsToLapTimeString((uint)Lap.last_lap_time);
    public string PitStopTimeString => PitStopTimeMs != 0 ? Formatter.FormatMsToPitStopString((uint)PitStopTimeMs) : string.Empty;
}

public partial class RaceLapsViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty] private int _raceEntryId;
    
    private ObservableCollection<LapDisplayModel> _laps;

    public ObservableCollection<LapDisplayModel> Laps
    {
        get => _laps;
        set => SetProperty(ref _laps, value);
    }
    
    public event Action GoBackRequested;

    public RaceLapsViewModel(DatabaseService databaseService, RaceEntry raceEntry)
    {
        _databaseService = databaseService;

        _raceEntryId = raceEntry.id;

        _ = LoadLaps();
    }

    [RelayCommand]
    private void GoBack()
    {
        GoBackRequested.Invoke();
    }
    
    private async Task LoadLaps()
    {
        var laps = await _databaseService.GetLaps(RaceEntryId);
        var pitStops = await _databaseService.GetPitStops(RaceEntryId);

        Laps = new ObservableCollection<LapDisplayModel>();

        foreach (var lap in laps)
        {
            Debug.WriteLine(lap.lap_number);
            var pit = pitStops.FirstOrDefault(p => p.lap_number == lap.lap_number);

            Laps.Add(new LapDisplayModel
            {
                Lap = lap,
                PitStopTimeMs = pit?.pit_stop_time ?? 0
            });
        }
    }
}