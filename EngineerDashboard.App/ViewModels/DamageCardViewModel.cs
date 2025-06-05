using System.Reactive.Disposables;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Services;
using EngineerDashboard.Telemetry.Data;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.ViewModels;

public partial class DamageCardViewModel : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();

    public DamageCardViewModel(TelemetryProvider telemetryProvider)
    {
        HookEvents(telemetryProvider);
    }

    private void HookEvents(TelemetryProvider telemetryProvider)
    {
        _telemetrySubscription.Add(
            telemetryProvider.CarTelemetryStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnCarTelemetryDataReceived));
        
        _telemetrySubscription.Add(
            telemetryProvider.CarDamageStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnCarDamageDataReceived));
    }

    private void OnCarTelemetryDataReceived(CarTelemetryPacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.carTelemetryData[playerId];
        
        // data.engineTemperature
    }

    private void OnCarDamageDataReceived(CarDamagePacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.carDamageData[playerId];
        
        // data.engineCEWear
        // data.engineDamage
        // data.engineESWear
        // data.engineICEWear
        // data.engineMGUHWear
        // data.engineMGUKWear
        // data.engineTCWear
        
        // data.diffuserDamage
        // data.drsFault
        // data.engineBlown
        // data.engineSeized
        // data.ersFault
        // data.floorDamage
        // data.frontLeftWingDamage
        // data.frontRightWingDamage
        // data.rearWingDamage
        // data.sidepodDamage
        // data.gearBoxDamage
    }
    
    public void Dispose()
    {
        _telemetrySubscription.Dispose();
    }
}