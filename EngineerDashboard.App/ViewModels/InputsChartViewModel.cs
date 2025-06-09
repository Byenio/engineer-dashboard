using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Helpers;
using EngineerDashboard.App.Services;
using EngineerDashboard.Telemetry.Data;
using EngineerDashboard.Telemetry.Packets;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EngineerDashboard.App.ViewModels;

public partial class InputsChartViewModel : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();
    private readonly SKTypeface _customTypeface;
    
    public Func<float, float> EasingFunction { get; set; }

    [ObservableProperty] private uint _bestLapTime;
    
    [ObservableProperty] private ObservableCollection<ObservablePoint> _throttleApplicationBestLap;
    [ObservableProperty] private ObservableCollection<ObservablePoint> _throttleApplicationCurrentLap;
    [ObservableProperty] private ObservableCollection<ObservablePoint> _brakeApplicationBestLap;
    [ObservableProperty] private ObservableCollection<ObservablePoint> _brakeApplicationCurrentLap;
    
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
            Labeler = value => $"{value:F0}%",
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
        }
    ];

    public InputsChartViewModel(TelemetryProvider telemetryProvider)
    {
        EasingFunction = t => 1;
        
        _customTypeface = FontHelper.LoadCustomFont();
        
        BestLapTime = 0;
        
        ThrottleApplicationBestLap = new ObservableCollection<ObservablePoint>();
        ThrottleApplicationCurrentLap = new ObservableCollection<ObservablePoint>();

        BrakeApplicationBestLap = new ObservableCollection<ObservablePoint>();
        BrakeApplicationCurrentLap = new ObservableCollection<ObservablePoint>();

        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = ThrottleApplicationBestLap,
                Name = "Throttle application on best lap",
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(0, 100, 42), 4),
                Fill = null,
                YToolTipLabelFormatter = point =>
                    $"{point.Coordinate.SecondaryValue:F0}m: {Math.Round(point.Coordinate.PrimaryValue, 0)}%"
            },
            new LineSeries<ObservablePoint>
            {
                Values = ThrottleApplicationCurrentLap,
                Name = "Throttle application on current lap",
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(0, 200, 83), 2),
                Fill = null,
                YToolTipLabelFormatter = point =>
                    $"{point.Coordinate.SecondaryValue:F0}m: {Math.Round(point.Coordinate.PrimaryValue, 0)}%"
            },
            new LineSeries<ObservablePoint>
            {
                Values = BrakeApplicationBestLap,
                Name = "Brake application on best lap",
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(120, 30, 30), 4),
                Fill = null,
                YToolTipLabelFormatter = point =>
                    $"{point.Coordinate.SecondaryValue:F0}m: {Math.Round(point.Coordinate.PrimaryValue, 0)}%"
            },
            new LineSeries<ObservablePoint>
            {
                Values = BrakeApplicationCurrentLap,
                Name = "Brake application on current lap",
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(244, 67, 54), 2),
                Fill = null,
                YToolTipLabelFormatter = point =>
                    $"{point.Coordinate.SecondaryValue:F0}m: {Math.Round(point.Coordinate.PrimaryValue, 0)}%"
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
                ThrottleApplicationBestLap.Clear();
                BrakeApplicationBestLap.Clear();
                
                foreach (var point in ThrottleApplicationCurrentLap)
                {
                    ThrottleApplicationBestLap.Add(new ObservablePoint(point.X, point.Y));
                }

                foreach (var point in BrakeApplicationCurrentLap)
                {
                    BrakeApplicationBestLap.Add(new ObservablePoint(point.X, point.Y));
                }
                
                BestLapTime = data.lastLapTimeInMS;
            }
            
            ThrottleApplicationCurrentLap.Clear();
            BrakeApplicationCurrentLap.Clear();

            LapNum = currentLapNum;
        }
        
        LapDistance = data.lapDistance;
    }

    private void OnCarTelemetryDataReceived(CarTelemetryPacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.carTelemetryData[playerId];
        
        ThrottleApplicationCurrentLap.Add(new ObservablePoint(LapDistance, Math.Round(data.throttle * 100, 0)));
        BrakeApplicationCurrentLap.Add(new ObservablePoint(LapDistance, Math.Round(data.brake * 100, 0)));
    }

    private void OnEventReceived(EventPacket packet)
    {
        if (packet.eventStringCode.ToString() == "SSTA")
        {
            ThrottleApplicationBestLap.Clear();
            ThrottleApplicationCurrentLap.Clear();
            BrakeApplicationBestLap.Clear();
            BrakeApplicationCurrentLap.Clear();
        }
    }

    partial void OnThrottleApplicationBestLapChanged(ObservableCollection<ObservablePoint> oldValue,
        ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }
    
    partial void OnThrottleApplicationCurrentLapChanged(ObservableCollection<ObservablePoint> oldValue,
        ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }

    partial void OnBrakeApplicationBestLapChanged(ObservableCollection<ObservablePoint> oldValue,
        ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }
    
    partial void OnBrakeApplicationCurrentLapChanged(ObservableCollection<ObservablePoint> oldValue,
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