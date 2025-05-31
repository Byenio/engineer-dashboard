using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Helpers;
using EngineerDashboard.App.Services;
using EngineerDashboard.Telemetry.Packets;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EngineerDashboard.App.ViewModels;

public partial class TyreWearChartViewModel : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();
    private readonly SKTypeface _customTypeface;
    
    [ObservableProperty] private ObservableCollection<ObservablePoint> _frontLeft;
    [ObservableProperty] private ObservableCollection<ObservablePoint> _frontRight;
    [ObservableProperty] private ObservableCollection<ObservablePoint> _rearLeft;
    [ObservableProperty] private ObservableCollection<ObservablePoint> _rearRight;
    [ObservableProperty] private ISeries[] _series;
    [ObservableProperty] private byte _numLaps = 0;
    [ObservableProperty] private bool _received = false;
    
    public Axis[] XAxes =>
    [
        new Axis
        {
            Name = "Tyre Wear",
            MinStep = 1,
            Labeler = value => $"{value:F0}",
            LabelsPaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                SKTypeface = _customTypeface
            },
            NamePaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                SKTypeface = _customTypeface
            }
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

    public TyreWearChartViewModel(TelemetryProvider telemetryProvider)
    {
        _customTypeface = LoadCustomFont();
        
        FrontLeft = new ObservableCollection<ObservablePoint>();
        FrontRight = new ObservableCollection<ObservablePoint>();
        RearLeft = new ObservableCollection<ObservablePoint>();
        RearRight = new ObservableCollection<ObservablePoint>();
        
        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = FrontLeft,
                Name = "Front Left",
                GeometrySize = 6,
                Stroke = new SolidColorPaint(new SKColor(31, 119, 180), 2),
                GeometryStroke = new SolidColorPaint(new SKColor(31, 119, 180), 2),
                Fill = null,
                YToolTipLabelFormatter = point => $"Lap {point.Coordinate.SecondaryValue:F0}: {Math.Round(point.Coordinate.PrimaryValue, 0)}%"
            },
            new LineSeries<ObservablePoint>
            {
                Values = FrontRight,
                Name = "Front Right",
                GeometrySize = 6,
                Stroke = new SolidColorPaint(new SKColor(95, 162, 220), 2),
                GeometryStroke = new SolidColorPaint(new SKColor(95, 162, 220), 2),
                Fill = null,
                YToolTipLabelFormatter = point => $"Lap {point.Coordinate.SecondaryValue:F0}: {Math.Round(point.Coordinate.PrimaryValue, 0)}%"
            },
            new LineSeries<ObservablePoint>
            {
                Values = RearLeft,
                Name = "Rear Left",
                GeometrySize = 6,
                Stroke = new SolidColorPaint(new SKColor(255, 127, 14), 2),
                GeometryStroke = new SolidColorPaint(new SKColor(255, 127, 14), 2),
                Fill = null,
                YToolTipLabelFormatter = point => $"Lap {point.Coordinate.SecondaryValue:F0}: {Math.Round(point.Coordinate.PrimaryValue, 0)}%"
            },
            new LineSeries<ObservablePoint>
            {
                Values = RearRight,
                Name = "Rear Right",
                GeometrySize = 6,
                Stroke = new SolidColorPaint(new SKColor(255, 174, 89), 2),
                GeometryStroke = new SolidColorPaint(new SKColor(255, 174, 89), 2),
                Fill = null,
                YToolTipLabelFormatter = point => $"Lap {point.Coordinate.SecondaryValue:F0}: {Math.Round(point.Coordinate.PrimaryValue, 0)}%"
            }
        };

        HookEvents(telemetryProvider);
    }

    private SKTypeface LoadCustomFont()
    {
        try
        {
            var uri = new Uri("pack://application:,,,/Assets/Fonts/JetBrainsMono-Regular.ttf");
            var info = System.Windows.Application.GetResourceStream(uri);
            if (info != null)
            {
                return SKTypeface.FromStream(info.Stream);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load custom font: {ex.Message}");
        }
        
        return SKTypeface.FromFamilyName("Consolas") ?? SKTypeface.Default;
    }

    private void HookEvents(TelemetryProvider telemetryProvider)
    {
        _telemetrySubscription.Add(
            telemetryProvider.LapDataStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnLapDataReceived));
        
        _telemetrySubscription.Add(
            telemetryProvider.CarDamageStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnCarDamageDataReceived));
    }

    private void OnLapDataReceived(LapDataPacket packet)
    {
        var playerId = packet.header.playerCarIndex;

        if (NumLaps > packet.lapData[playerId].currentLapNum)
        {
            FrontLeft.Clear();
            FrontRight.Clear();
            RearLeft.Clear();
            RearRight.Clear();
        }

        if (NumLaps == packet.lapData[playerId].currentLapNum) return;
        
        NumLaps = packet.lapData[playerId].currentLapNum;
        Received = false;
    }

    private void OnCarDamageDataReceived(CarDamagePacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.carDamageData[playerId];
        
        if (NumLaps > 1 && !Received)
        {
            FrontLeft.Add(new ObservablePoint(NumLaps - 1, Math.Round(data.tyresWear[2], 0)));
            FrontRight.Add(new ObservablePoint(NumLaps - 1, Math.Round(data.tyresWear[3], 0)));
            RearLeft.Add(new ObservablePoint(NumLaps - 1, Math.Round(data.tyresWear[0], 0)));
            RearRight.Add(new ObservablePoint(NumLaps - 1, Math.Round(data.tyresWear[1], 0)));
            
            Received = true;
        }

    }

    partial void OnFrontLeftChanged(ObservableCollection<ObservablePoint> oldValue, ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }
    partial void OnFrontRightChanged(ObservableCollection<ObservablePoint> oldValue, ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }
    partial void OnRearLeftChanged(ObservableCollection<ObservablePoint> oldValue, ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }
    partial void OnRearRightChanged(ObservableCollection<ObservablePoint> oldValue, ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }

    public void Dispose()
    {
        _customTypeface?.Dispose();
        _telemetrySubscription.Dispose();
    }
}