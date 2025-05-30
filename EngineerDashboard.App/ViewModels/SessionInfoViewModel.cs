using System.Reactive.Disposables;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Services;
using EngineerDashboard.Telemetry;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.ViewModels;

public partial class SessionInfoViewModel : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();

    [ObservableProperty] private Session _sessionType;
    [ObservableProperty] private Track _track;
    [ObservableProperty] private byte _currentLap;
    [ObservableProperty] private byte _totalLaps;
    [ObservableProperty] private Weather _weather;
    [ObservableProperty] private sbyte _trackTemperature;
    [ObservableProperty] private sbyte _airTemperature;
    [ObservableProperty] private SafetyCarStatus _safetyCarStatus;

    [ObservableProperty] private bool _showSafetyCarStatusString = false;
    [ObservableProperty] private bool _showTemperaturesString = true;

    public string SessionTypeOnTrackString => new string($"[ {SessionType} @ {Track} ]");
    public string LapOfLapsString => new string($"[ lap: {CurrentLap}/{TotalLaps} ]");
    public string WeatherString => new string($"[ weather: {Weather} ]");
    public string TemperaturesString => new string($"[ air: {AirTemperature}°C | track: {TrackTemperature}°C ]");
    public string SafetyCarStatusString => new string($"[ {SafetyCarStatus} ]");
    
    public SessionInfoViewModel(TelemetryProvider telemetryProvider)
    {
        HookEvents(telemetryProvider);
    }

    private void HookEvents(TelemetryProvider telemetryProvider)
    {
        _telemetrySubscription.Add(
            telemetryProvider.SessionStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnSessionDataReceived)
            );

        _telemetrySubscription.Add(
            telemetryProvider.LapDataStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnLapDataReceived));
    }

    private void OnSessionDataReceived(SessionPacket packet)
    {
        SessionType = packet.sessionType;
        Track = packet.trackId;
        TotalLaps = packet.totalLaps;
        Weather = packet.weather;
        SafetyCarStatus = packet.safetyCarStatus;
        TrackTemperature = packet.trackTemperature;
        AirTemperature = packet.airTemperature;
    }

    private void OnLapDataReceived(LapDataPacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        
        CurrentLap = packet.lapData[playerId].currentLapNum;
    }

    partial void OnTotalLapsChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(LapOfLapsString));
    }

    partial void OnCurrentLapChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(LapOfLapsString));
    }

    partial void OnSessionTypeChanged(Session oldValue, Session newValue)
    {
        OnPropertyChanged(nameof(SessionTypeOnTrackString));
    }

    partial void OnTrackChanged(Track oldValue, Track newValue)
    {
        OnPropertyChanged(nameof(SessionTypeOnTrackString));
    }

    partial void OnAirTemperatureChanged(sbyte oldValue, sbyte newValue)
    {
        OnPropertyChanged(nameof(TemperaturesString));
    }

    partial void OnTrackTemperatureChanged(sbyte oldValue, sbyte newValue)
    {
        OnPropertyChanged(nameof(TemperaturesString));
    }

    partial void OnWeatherChanged(Weather oldValue, Weather newValue)
    {
        OnPropertyChanged(nameof(WeatherString));
    }

    partial void OnSafetyCarStatusChanged(SafetyCarStatus oldValue, SafetyCarStatus newValue)
    {
        ShowSafetyCarStatusString = newValue != SafetyCarStatus.NONE;
        ShowTemperaturesString = !ShowSafetyCarStatusString;
        OnPropertyChanged(nameof(SafetyCarStatusString));
    }
    
    public void Dispose()
    {
        _telemetrySubscription.Dispose();
    }
}