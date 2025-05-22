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
    [ObservableProperty] private byte _totalLaps;
    [ObservableProperty] private sbyte _trackTemperature;
    [ObservableProperty] private sbyte _airTemperature;
    
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
    }

    private void OnSessionDataReceived(SessionPacket packet)
    {
        SessionType = packet.sessionType;
        TotalLaps = packet.totalLaps;
        TrackTemperature = packet.trackTemperature;
        AirTemperature = packet.airTemperature;
    }

    public void Dispose()
    {
        _telemetrySubscription.Dispose();
    }
}