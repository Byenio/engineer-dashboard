using System.Reactive.Disposables;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Services;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private ushort _speed;
    
    private readonly CompositeDisposable _telemetryClientSubscription = new();

    private void OnCarTelemetryDataReceive(CarTelemetryPacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        Speed = packet.carTelemetryData[playerId].speed;
    }
    
    private void HookEvents(TelemetryProvider telemetryProvider)
    {
        var uiScheduler = SynchronizationContext.Current;
        
        _telemetryClientSubscription.Add(
            telemetryProvider.CarTelemetryStream
                .ObserveOn(uiScheduler)
                .Subscribe(OnCarTelemetryDataReceive)
            );
    }
    
    public MainWindowViewModel(TelemetryProvider telemetryProvider)
    {
        HookEvents(telemetryProvider);
    }

    public void Dispose()
    {
        _telemetryClientSubscription.Dispose();
    }
}