using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Helpers;
using EngineerDashboard.App.Services;
using EngineerDashboard.Telemetry.Packets;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EngineerDashboard.App.ViewModels;

public partial class TelemetryChartViewModel : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();
    private readonly SKTypeface _customTypeface;
    
    public Func<float, float> EasingFunction { get; set; }

    [ObservableProperty] private uint _bestLapTime;
    
    [ObservableProperty] private ObservableCollection<ObservablePoint> _speedBestLap;
    [ObservableProperty] private ObservableCollection<ObservablePoint> _speedCurrentLap;
    [ObservableProperty] private ObservableCollection<ObservablePoint> _drsBestLap;
    [ObservableProperty] private ObservableCollection<ObservablePoint> _drsCurrentLap;
    [ObservableProperty] private ObservableCollection<ObservablePoint> _gearBestLap;
    [ObservableProperty] private ObservableCollection<ObservablePoint> _gearCurrentLap;
    
    [ObservableProperty] private ISeries[] _series;

    [ObservableProperty] private byte _lapNum;
    [ObservableProperty] private float _lapDistance;

    public Axis[] XAxes =>
    [
        new Axis
        {
            NamePaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                SKTypeface = _customTypeface
            },
            LabelsPaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                SKTypeface = _customTypeface,
            },
        }
    ];

    public Axis[] YAxes =>
    [
        new Axis
        {
            Labeler = value => $"{value:F0} kmh",
            LabelsPaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                SKTypeface = _customTypeface
            },
            NamePaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                SKTypeface = _customTypeface
            },
            SeparatorsPaint = new SolidColorPaint
            {
                Color = new SKColor(48, 48, 48)
            }
        },
        new Axis
        {
            Labeler = value => $"{value:F0}",
            LabelsPaint = new SolidColorPaint
            {
                Color = SKColors.Transparent,
                SKTypeface = _customTypeface
            },
            NamePaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                SKTypeface = _customTypeface
            },
            SeparatorsPaint = new SolidColorPaint
            {
                Color = SKColors.Transparent
            }
        },
        new Axis
        {
            Labeler = value => $"{value} gear",
            LabelsPaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                SKTypeface = _customTypeface
            },
            NamePaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                SKTypeface = _customTypeface
            },
            SeparatorsPaint = new SolidColorPaint
            {
                Color = SKColors.Transparent
            },
            Position = AxisPosition.End,
            MaxLimit = 8
        }
    ];

    public TelemetryChartViewModel(TelemetryProvider telemetryProvider)
    {
        EasingFunction = t => 1;
        
        _customTypeface = FontHelper.LoadCustomFont();
        
        BestLapTime = 0;
        
        SpeedBestLap = new ObservableCollection<ObservablePoint>();
        SpeedCurrentLap = new ObservableCollection<ObservablePoint>();

        DrsBestLap = new ObservableCollection<ObservablePoint>();
        DrsCurrentLap = new ObservableCollection<ObservablePoint>();
        
        GearBestLap = new ObservableCollection<ObservablePoint>();
        GearCurrentLap = new ObservableCollection<ObservablePoint>();

        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = SpeedBestLap,
                Name = "Speed on best lap",
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(153, 117, 0), 4),
                Fill = null,
                YToolTipLabelFormatter = point =>
                    $"{point.Coordinate.SecondaryValue:F0}m: {Math.Round(point.Coordinate.PrimaryValue, 0)} kmh",
                ScalesYAt = 0
            },
            new LineSeries<ObservablePoint>
            {
                Values = SpeedCurrentLap,
                Name = "Speed on current lap",
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(255, 193, 7), 2),
                Fill = null,
                YToolTipLabelFormatter = point =>
                    $"{point.Coordinate.SecondaryValue:F0}m: {Math.Round(point.Coordinate.PrimaryValue, 0)} kmh",
                ScalesYAt = 0
            },
            new LineSeries<ObservablePoint>
            {
                Values = DrsBestLap,
                Name = "DRS on best lap",
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(2, 85, 122), 4),
                Fill = null,
                YToolTipLabelFormatter = point =>
                    $"{point.Coordinate.SecondaryValue:F0}m: {point.Coordinate.PrimaryValue}",
                ScalesYAt = 1,
                LineSmoothness = 0
            },
            new LineSeries<ObservablePoint>
            {
                Values = DrsCurrentLap,
                Name = "DRS on current lap",
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(3, 169, 244), 2),
                Fill = null,
                YToolTipLabelFormatter = point =>
                    $"{point.Coordinate.SecondaryValue:F0}m: {point.Coordinate.PrimaryValue}",
                ScalesYAt = 1,
                LineSmoothness = 0
            },
            new LineSeries<ObservablePoint>
            {
                Values = GearBestLap,
                Name = "Gear on best lap",
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(80, 20, 90), 4),
                Fill = null,
                YToolTipLabelFormatter = point =>
                    $"{point.Coordinate.SecondaryValue:F0}m: {point.Coordinate.PrimaryValue} gear",
                ScalesYAt = 2,
                LineSmoothness = 0
            },
            new LineSeries<ObservablePoint>
            {
                Values = GearCurrentLap,
                Name = "Gear on current lap",
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(156, 39, 176), 2),
                Fill = null,
                YToolTipLabelFormatter = point =>
                    $"{point.Coordinate.SecondaryValue:F0}m: {point.Coordinate.PrimaryValue} gear",
                ScalesYAt = 2,
                LineSmoothness = 0
            }
        };
        
        HookEvents(telemetryProvider);
    }

    private void HookEvents(TelemetryProvider telemetryProvider)
    {
        _telemetrySubscription.Add(
            telemetryProvider.CarTelemetryStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnCarTelemetryDataReceived));

        _telemetrySubscription.Add(
            telemetryProvider.LapDataStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnLapDataReceived));
        
        _telemetrySubscription.Add(
            telemetryProvider.EventStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnEventReceived));
    }

    private void OnLapDataReceived(LapDataPacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.lapData[playerId];

        var currentLapNum = data.currentLapNum;

        if (LapNum < currentLapNum)
        {
            if (data.lastLapTimeInMS < BestLapTime || BestLapTime == 0)
            {
                SpeedBestLap.Clear();
                DrsBestLap.Clear();
                GearBestLap.Clear();
                
                foreach (var point in SpeedCurrentLap)
                {
                    SpeedBestLap.Add(new ObservablePoint(point.X, point.Y));
                }
                
                foreach (var point in DrsCurrentLap)
                {
                    DrsBestLap.Add(new ObservablePoint(point.X, point.Y));
                }

                foreach (var point in GearCurrentLap)
                {
                    GearBestLap.Add(new ObservablePoint(point.X, point.Y));
                }
                
                BestLapTime = data.lastLapTimeInMS;
            }
            
            SpeedCurrentLap.Clear();
            DrsCurrentLap.Clear();
            GearCurrentLap.Clear();

            LapNum = currentLapNum;
        }
        
        LapDistance = data.lapDistance;
    }

    private void OnCarTelemetryDataReceived(CarTelemetryPacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.carTelemetryData[playerId];
        
        SpeedCurrentLap.Add(new ObservablePoint(LapDistance, data.speed));
        DrsCurrentLap.Add(new ObservablePoint(LapDistance, data.drs));
        GearCurrentLap.Add(new ObservablePoint(LapDistance, data.gear));
    }

    private void OnEventReceived(EventPacket packet)
    {
        var stringCode = new string(packet.eventStringCode).TrimEnd('\0');
        
        if (stringCode == "SSTA")
        {
            SpeedBestLap.Clear();
            SpeedCurrentLap.Clear();
            DrsBestLap.Clear();
            DrsCurrentLap.Clear();
            GearBestLap.Clear();
            GearCurrentLap.Clear();
            BestLapTime = 0;
            LapNum = 0;
        }
    }

    partial void OnSpeedBestLapChanged(ObservableCollection<ObservablePoint> oldValue,
        ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }
    
    partial void OnSpeedCurrentLapChanged(ObservableCollection<ObservablePoint> oldValue,
        ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }
    
    partial void OnDrsBestLapChanged(ObservableCollection<ObservablePoint> oldValue,
        ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }
    
    partial void OnDrsCurrentLapChanged(ObservableCollection<ObservablePoint> oldValue,
        ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }
    
    partial void OnGearBestLapChanged(ObservableCollection<ObservablePoint> oldValue,
        ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }
    
    partial void OnGearCurrentLapChanged(ObservableCollection<ObservablePoint> oldValue,
        ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }
    
    public void Dispose()
    {
        _customTypeface.Dispose();
        _telemetrySubscription.Dispose();
    }
}