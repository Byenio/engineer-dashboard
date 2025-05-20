using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Services;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private ushort _speed;
    
    private readonly TelemetryService _telemetryService;

    private void OnCarTelemetryDataReceive(CarTelemetryPacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        Speed = packet.carTelemetryData[playerId].speed;
    }
    
    private void HookEvents()
    {
        _telemetryService.TelemetryClient.OnCarTelemetryDataReceive += OnCarTelemetryDataReceive;
    }
    
    public MainWindowViewModel(TelemetryService telemetryService)
    {
        _telemetryService = telemetryService;

        HookEvents();
    }
}