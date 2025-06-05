using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Helpers;
using EngineerDashboard.App.Services;
using EngineerDashboard.Telemetry;
using EngineerDashboard.Telemetry.Data;
using EngineerDashboard.Telemetry.Packets;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EngineerDashboard.App.ViewModels;

public partial class CarInfoCardViewModel : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();
    private readonly SKTypeface _customTypeface;

    [ObservableProperty] private byte _drsActive;
    [ObservableProperty] private ushort _speed;
    [ObservableProperty] private byte _brakeApplication;
    [ObservableProperty] private byte _throttleApplication;
    [ObservableProperty] private sbyte _gear;
    [ObservableProperty] private byte _ersDeployMode;
    [ObservableProperty] private byte _ersStoreEnergy;
    [ObservableProperty] private byte _pitLimiterStatus;
    [ObservableProperty] private ZoneFlag _vehicleFiaFlag;
    [ObservableProperty] private ObservableCollection<TyreSetData> _tyreSets;
    [ObservableProperty] private byte _fittedTyreIdx;
    
    public ISeries[] ThrottleSeries { get; set; }
    public ISeries[] BrakeSeries { get; set; }
    public ISeries[] SpeedSeries { get; set; }
    public ISeries[] ErsSeries { get; set; }

    public Brush DrsActiveColor => ColorHelper.GetDrsBrush(0, DrsActive);
    public string SpeedString => $"{Speed} kmh";
    public string BrakeApplicationString => $"{BrakeApplication}%";
    public string ThrottleApplicationString => $"{ThrottleApplication}%";
    public string GearString => $"{Gear} gear";

    public string ErsDeployModeString => ErsDeployMode switch
    {
        0 => "NONE",
        1 => "MEDIUM",
        2 => "HOTLAP",
        3 => "OVERTAKE",
        _ => "NONE"
    };
    public Brush PitLimiterColor => ColorHelper.GetPitBrush(PitLimiterStatus);
    public Brush VehicleFiaFlagColor => ColorHelper.GetFlagBrush(VehicleFiaFlag);

    public CarInfoCardViewModel(TelemetryProvider telemetryProvider)
    {
        _customTypeface = FontHelper.LoadCustomFont();
        
        HookEvents(telemetryProvider);

        DrsActive = 0;
        Speed = 0;
        BrakeApplication = 0;
        ThrottleApplication = 0;
        Gear = 0;
        ErsDeployMode = 0;
        ErsStoreEnergy = 0;
        PitLimiterStatus = 0;
        VehicleFiaFlag = ZoneFlag.NONE;
        TyreSets = new ObservableCollection<TyreSetData>();
        FittedTyreIdx = 0;
        
        ThrottleSeries = new ISeries[]
        {
            new PieSeries<double>
            {
                Values = new double[] { ThrottleApplication },
                MaxRadialColumnWidth = 40,
                InnerRadius = 40,
                Fill = new SolidColorPaint(SKColors.LimeGreen),
                DataLabelsPaint = new SolidColorPaint
                {
                    Color = SKColors.White,
                    SKTypeface = _customTypeface
                },
                DataLabelsSize = 16,
                DataLabelsPosition = PolarLabelsPosition.ChartCenter,
            },
            new PieSeries<double>
            {
                Values = new double[] { 100 - ThrottleApplication },
                MaxRadialColumnWidth = 40,
                InnerRadius = 40,
                Fill = new SolidColorPaint(new SKColor(60, 60, 60)),
                IsVisible = true
            }
        };

        BrakeSeries = new ISeries[]
        {
            new PieSeries<double>
            {
                Values = new double[] { BrakeApplication },
                MaxRadialColumnWidth = 40,
                InnerRadius = 40,
                Fill = new SolidColorPaint(SKColors.Red),
                DataLabelsPaint = new SolidColorPaint
                {
                    Color = SKColors.White,
                    SKTypeface = _customTypeface
                },
                DataLabelsSize = 16,
                DataLabelsPosition = PolarLabelsPosition.ChartCenter,
            },
            new PieSeries<double>
            {
                Values = new double[] { 100 - BrakeApplication },
                MaxRadialColumnWidth = 40,
                InnerRadius = 40,
                Fill = new SolidColorPaint(new SKColor(60, 60, 60)),
                IsVisible = true
            }
        };
        
        SpeedSeries = new ISeries[]
        {
            new PieSeries<double>
            {
                Values = new double[] { Speed },
                MaxRadialColumnWidth = 40,
                InnerRadius = 40,
                Fill = new SolidColorPaint(SKColors.White),
                DataLabelsPaint = new SolidColorPaint
                {
                    Color = SKColors.White,
                    SKTypeface = _customTypeface
                },
                DataLabelsSize = 16,
                DataLabelsPosition = PolarLabelsPosition.ChartCenter,
            },
            new PieSeries<double>
            {
                Values = new double[] { 380 - Speed },
                MaxRadialColumnWidth = 40,
                InnerRadius = 40,
                Fill = new SolidColorPaint(new SKColor(60, 60, 60)),
                IsVisible = true
            }
        };
        
        ErsSeries = new ISeries[]
        {
            new PieSeries<double>
            {
                Values = new double[] { ErsStoreEnergy },
                MaxRadialColumnWidth = 40,
                InnerRadius = 40,
                Fill = new SolidColorPaint(SKColors.Gold),
                DataLabelsPaint = new SolidColorPaint
                {
                    Color = SKColors.White,
                    SKTypeface = _customTypeface
                },
                DataLabelsSize = 16,
                DataLabelsPosition = PolarLabelsPosition.ChartCenter,
            },
            new PieSeries<double>
            {
                Values = new double[] { 100 - ErsStoreEnergy },
                MaxRadialColumnWidth = 40,
                InnerRadius = 40,
                Fill = new SolidColorPaint(new SKColor(60, 60, 60)),
                IsVisible = true
            }
        };
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
            telemetryProvider.TyreSetStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnTyreSetDataReceived));
    }

    private void OnCarTelemetryDataReceived(CarTelemetryPacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.carTelemetryData[playerId];

        DrsActive = data.drs;
        Speed = data.speed;
        BrakeApplication = (byte)Math.Round(data.brake * 100, 0);
        ThrottleApplication = (byte)Math.Round(data.throttle * 100, 0);
        Gear = data.gear;
    }

    private void OnCarStatusDataReceived(CarStatusPacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.carStatusData[playerId];

        ErsDeployMode = data.ersDeployMode;
        ErsStoreEnergy = (byte)Math.Round(data.ersStoreEnergy / 40000, 0);
        PitLimiterStatus = data.pitLimiterStatus;
        VehicleFiaFlag = data.vehicleFiaFlags;
    }
    
    private void OnTyreSetDataReceived(TyreSetPacket packet)
    {
        var playerId = packet.header.playerCarIndex;

        if (playerId == packet.carIdx)
        {
            FittedTyreIdx = packet.fittedIdx;
            var tyres = packet.data;

            TyreSets.Clear();
            
            foreach (var tyre in tyres)
            {
                TyreSets.Add(tyre);
            }
        }
    }

    #region OnPropertyChanged handlers
    
    partial void OnDrsActiveChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(DrsActiveColor));
    }

    partial void OnSpeedChanged(ushort oldValue, ushort newValue)
    {
        OnPropertyChanged(nameof(SpeedString));
        
        if (SpeedSeries[0] is PieSeries<double> filled &&
            SpeedSeries[1] is PieSeries<double> background)
        {
            filled.Values = new[] { (double)newValue };
            background.Values = new[] { 380 - (double)newValue };
        }

        OnPropertyChanged(nameof(SpeedSeries));
    }

    partial void OnBrakeApplicationChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(BrakeApplicationString));
        
        if (BrakeSeries[0] is PieSeries<double> filled &&
            BrakeSeries[1] is PieSeries<double> background)
        {
            filled.Values = new[] { (double)newValue };
            background.Values = new[] { 100 - (double)newValue };
        }

        OnPropertyChanged(nameof(BrakeSeries));
    }

    partial void OnThrottleApplicationChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(ThrottleApplicationString));

        if (ThrottleSeries[0] is PieSeries<double> filled &&
            ThrottleSeries[1] is PieSeries<double> background)
        {
            filled.Values = new[] { (double)newValue };
            background.Values = new[] { 100 - (double)newValue };
        }

        OnPropertyChanged(nameof(ThrottleSeries));
    }


    partial void OnGearChanged(sbyte oldValue, sbyte newValue)
    {
        OnPropertyChanged(nameof(GearString));
    }

    partial void OnErsStoreEnergyChanged(byte oldValue, byte newValue)
    {
        if (ErsSeries[0] is PieSeries<double> filled &&
            ErsSeries[1] is PieSeries<double> background)
        {
            filled.Values = new[] { (double)newValue };
            background.Values = new[] { 100 - (double)newValue };
        }

        OnPropertyChanged(nameof(ErsSeries));
    }

    partial void OnErsDeployModeChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(ErsDeployModeString));
    }

    partial void OnPitLimiterStatusChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(PitLimiterColor));
    }

    partial void OnVehicleFiaFlagChanged(ZoneFlag oldValue, ZoneFlag newValue)
    {
        OnPropertyChanged(nameof(VehicleFiaFlagColor));
    }
    
    #endregion
    
    public void Dispose()
    {
        _telemetrySubscription.Dispose();
    }
}