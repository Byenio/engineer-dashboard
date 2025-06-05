using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Helpers;
using EngineerDashboard.App.Services;
using EngineerDashboard.Telemetry.Data;
using EngineerDashboard.Telemetry.Packets;
using SkiaSharp;

namespace EngineerDashboard.App.ViewModels;

public partial class DamageCardViewModel : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();
    private readonly SKTypeface _customTypeface;

    [ObservableProperty] private byte _frontLeftWingDamage;
    [ObservableProperty] private byte _frontRightWingDamage;
    [ObservableProperty] private byte _floorDamage;
    [ObservableProperty] private byte _rearWingDamage;
    [ObservableProperty] private ushort _engineTemperature;

    public Brush FrontLeftWingDamageColor => ColorHelper.GetDamageColor(FrontLeftWingDamage);
    public Brush FrontRightWingDamageColor => ColorHelper.GetDamageColor(FrontRightWingDamage);
    public Brush FloorDamageColor => ColorHelper.GetDamageColor(FloorDamage);
    public Brush RearWingDamageColor => ColorHelper.GetDamageColor(RearWingDamage);

    public string FrontLeftWingDamageString => $"{FrontLeftWingDamage}%";
    public string FrontRightWingDamageString => $"{FrontRightWingDamage}%";
    public string FloorDamageString => $"{FloorDamage}%";
    public string RearWingDamageString => $"{RearWingDamage}%";
    
    public DamageCardViewModel(TelemetryProvider telemetryProvider)
    {
        HookEvents(telemetryProvider);

        FrontLeftWingDamage = 0;
        FrontRightWingDamage = 0;
        FloorDamage = 0;
        RearWingDamage = 0;
        EngineTemperature = 0;
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

        EngineTemperature = data.engineTemperature;
    }

    private void OnCarDamageDataReceived(CarDamagePacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.carDamageData[playerId];

        FrontLeftWingDamage = data.frontLeftWingDamage;
        FrontRightWingDamage = data.frontRightWingDamage;
        FloorDamage = data.floorDamage;
        RearWingDamage = data.rearWingDamage;
    }

    partial void OnFrontLeftWingDamageChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(FrontLeftWingDamageColor));
        OnPropertyChanged(nameof(FrontLeftWingDamageString));
    }

    partial void OnFrontRightWingDamageChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(FrontRightWingDamageColor));
        OnPropertyChanged(nameof(FrontRightWingDamageString));
    }
    
    partial void OnFloorDamageChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(FloorDamageColor));
        OnPropertyChanged(nameof(FloorDamageString));
    }

    partial void OnRearWingDamageChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(RearWingDamageColor));
        OnPropertyChanged(nameof(RearWingDamageString));
    }

    public void Dispose()
    {
        _telemetrySubscription.Dispose();
    }
}