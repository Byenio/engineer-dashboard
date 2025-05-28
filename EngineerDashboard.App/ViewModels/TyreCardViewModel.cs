using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Helpers;
using EngineerDashboard.App.Services;
using EngineerDashboard.Telemetry;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.ViewModels;

public partial class TyreCardViewModel : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();
    
    [ObservableProperty] private float[] _tyresWear;
    [ObservableProperty] private byte[] _tyresInnerTemperature;
    [ObservableProperty] private byte[] _tyresSurfaceTemperature;
    [ObservableProperty] private float[] _tyresPressure;
    [ObservableProperty] private VisualTyreCompound _visualTyreCompound;
    [ObservableProperty] private TyreCompound _actualTyreCompound;
    [ObservableProperty] private byte _tyresAgeLaps;

    public string CurrentTyreCompoundString => new string($"({VisualTyreCompound.ToString()} - {ActualTyreCompound})");
    public string CurrentTyreAgeLapsString => new string($"{TyresAgeLaps} laps old");
    public string[] TyresWearString =>
    [
        new string($"{Math.Round(TyresWear[0], 0)}%"),
        new string($"{Math.Round(TyresWear[1], 0)}%"),
        new string($"{Math.Round(TyresWear[2], 0)}%"),
        new string($"{Math.Round(TyresWear[3], 0)}%")
    ];
    public string[] TyresInnerTemperatureString =>
    [
        new string($"{TyresInnerTemperature[0]}°C"),
        new string($"{TyresInnerTemperature[1]}°C"),
        new string($"{TyresInnerTemperature[2]}°C"),
        new string($"{TyresInnerTemperature[3]}°C")
    ];
    public string[] TyresSurfaceTemperatureString =>
    [
        new string($"{TyresSurfaceTemperature[0]}°C"),
        new string($"{TyresSurfaceTemperature[1]}°C"),
        new string($"{TyresSurfaceTemperature[2]}°C"),
        new string($"{TyresSurfaceTemperature[3]}°C")
    ];
    public string[] TyresPressureString =>
    [
        new string($"{Math.Round(TyresPressure[0], 1):F1}psi"),
        new string($"{Math.Round(TyresPressure[1], 1):F1}psi"),
        new string($"{Math.Round(TyresPressure[2], 1):F1}psi"),
        new string($"{Math.Round(TyresPressure[3], 1):F1}psi")
    ];

    public Brush[] TyresInnerTemperatureColor =>
    [
        ColorHelper.GetTyreTemperatureBrush(ActualTyreCompound, TyresInnerTemperature[0]),
        ColorHelper.GetTyreTemperatureBrush(ActualTyreCompound, TyresInnerTemperature[1]),
        ColorHelper.GetTyreTemperatureBrush(ActualTyreCompound, TyresInnerTemperature[2]),
        ColorHelper.GetTyreTemperatureBrush(ActualTyreCompound, TyresInnerTemperature[3])
    ];
    
    public Brush[] TyresSurfaceTemperatureColor =>
    [
        ColorHelper.GetTyreTemperatureBrush(ActualTyreCompound, TyresSurfaceTemperature[0]),
        ColorHelper.GetTyreTemperatureBrush(ActualTyreCompound, TyresSurfaceTemperature[1]),
        ColorHelper.GetTyreTemperatureBrush(ActualTyreCompound, TyresSurfaceTemperature[2]),
        ColorHelper.GetTyreTemperatureBrush(ActualTyreCompound, TyresSurfaceTemperature[3])
    ];

    public Brush[] TyresWearColor =>
    [
        ColorHelper.GetTyreWearBrush(TyresWear[0]),
        ColorHelper.GetTyreWearBrush(TyresWear[1]),
        ColorHelper.GetTyreWearBrush(TyresWear[2]),
        ColorHelper.GetTyreWearBrush(TyresWear[3])
    ];
    
    public Brush VisualTyreCompoundColor => ColorHelper.GetTyreBrush(VisualTyreCompound);

    public TyreCardViewModel(TelemetryProvider telemetryProvider)
    {
        HookEvents(telemetryProvider);
        
        TyresWear = [0, 0, 0, 0];
        TyresInnerTemperature = [0, 0, 0, 0];
        TyresSurfaceTemperature = [0, 0, 0, 0];
        TyresPressure = [0, 0, 0, 0];
        VisualTyreCompound = VisualTyreCompound.SOFT;
        ActualTyreCompound = TyreCompound.C5;
        TyresAgeLaps = 0;
    }

    private void HookEvents(TelemetryProvider telemetryProvider)
    {
        _telemetrySubscription.Add(
            telemetryProvider.CarTelemetryStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnCarTelemetryDataReceived));
        
        _telemetrySubscription.Add(
            telemetryProvider.CarStatusStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnCarStatusDataReceived));
        
        _telemetrySubscription.Add(
            telemetryProvider.CarDamageStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnCarDamageDataReceived));
    }

    private void OnCarTelemetryDataReceived(CarTelemetryPacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.carTelemetryData[playerId];

        TyresPressure = data.tyresPressure;
        TyresInnerTemperature = data.tyresInnerTemperature;
        TyresSurfaceTemperature = data.tyresSurfaceTemperature;
    }

    private void OnCarStatusDataReceived(CarStatusPacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.carStatusData[playerId];

        TyresAgeLaps = data.tyresAgeLaps;
        VisualTyreCompound = data.visualTyreCompound;
        ActualTyreCompound = data.actualTyreCompound;
    }

    private void OnCarDamageDataReceived(CarDamagePacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.carDamageData[playerId];
        
        TyresWear = data.tyresWear;
    }
    
    #region OnPropertyChanged handlers

    partial void OnTyresWearChanged(float[] oldValue, float[] newValue)
    {
        OnPropertyChanged(nameof(TyresWearString));
        OnPropertyChanged(nameof(TyresWearColor));
    }

    partial void OnTyresInnerTemperatureChanged(byte[] oldValue, byte[] newValue)
    {
        OnPropertyChanged(nameof(TyresInnerTemperatureString));
        OnPropertyChanged(nameof(TyresInnerTemperatureColor));
    }

    partial void OnTyresSurfaceTemperatureChanged(byte[] oldValue, byte[] newValue)
    {
        OnPropertyChanged(nameof(TyresSurfaceTemperatureString));
        OnPropertyChanged(nameof(TyresSurfaceTemperatureColor));
    }

    partial void OnTyresPressureChanged(float[] oldValue, float[] newValue)
    {
        OnPropertyChanged(nameof(TyresPressureString));
    }

    partial void OnVisualTyreCompoundChanged(VisualTyreCompound oldValue, VisualTyreCompound newValue)
    {
        OnPropertyChanged(nameof(CurrentTyreCompoundString));
        OnPropertyChanged(nameof(VisualTyreCompoundColor));
    }

    partial void OnActualTyreCompoundChanged(TyreCompound oldValue, TyreCompound newValue)
    {
        OnPropertyChanged(nameof(CurrentTyreCompoundString));
        OnPropertyChanged(nameof(VisualTyreCompoundColor));
    }

    partial void OnTyresAgeLapsChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(CurrentTyreCompoundString));
        OnPropertyChanged(nameof(CurrentTyreAgeLapsString));
    }
    
    #endregion

    public void Dispose()
    {
        _telemetrySubscription.Dispose();
    }
}