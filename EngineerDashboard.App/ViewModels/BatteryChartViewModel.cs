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

public partial class BatteryChartViewModel : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();
    private readonly SKTypeface _customTypeface;
    
    [ObservableProperty] private ObservableCollection<ObservablePoint> _ers;
    [ObservableProperty] private ISeries[] _series;
    [ObservableProperty] private byte _numLaps = 0;
    [ObservableProperty] private bool _received = false;
    
    public Axis[] XAxes =>
    [
        new Axis
        {
            Name = "Battery usage",
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

    public BatteryChartViewModel(TelemetryProvider telemetryProvider)
    {
        _customTypeface = FontHelper.LoadCustomFont();
        
        Ers = new ObservableCollection<ObservablePoint>();
        
        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = Ers,
                Name = "Ers",
                GeometrySize = 6,
                Stroke = new SolidColorPaint(new SKColor(31, 119, 180), 2),
                GeometryStroke = new SolidColorPaint(new SKColor(31, 119, 180), 2),
                Fill = null,
                YToolTipLabelFormatter = point => $"Lap {point.Coordinate.SecondaryValue:F0}: {Math.Round(point.Coordinate.PrimaryValue, 2)}%"
            }
        };

        HookEvents(telemetryProvider);
    }
    private void HookEvents(TelemetryProvider telemetryProvider)
    {
        _telemetrySubscription.Add(
            telemetryProvider.LapDataStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnLapDataReceived));
        
        _telemetrySubscription.Add(
            telemetryProvider.CarStatusStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnCarStatusDataReceived));
    }

    private void OnLapDataReceived(LapDataPacket packet)
    {
        var playerId = packet.header.playerCarIndex;

        if (NumLaps > packet.lapData[playerId].currentLapNum)
        {
            Ers.Clear();
        }

        if (NumLaps == packet.lapData[playerId].currentLapNum) return;
        
        NumLaps = packet.lapData[playerId].currentLapNum;
        Received = false;
    }

    private void OnCarStatusDataReceived(CarStatusPacket packet)
    {
        var playerId = packet.header.playerCarIndex;
        var data = packet.carStatusData[playerId];
        
        if (NumLaps > 1 && !Received)
        {
            Ers.Add(new ObservablePoint(NumLaps - 1, Math.Round(data.ersStoreEnergy / 40000, 2)));
            
            Received = true;
        }

    }

    partial void OnErsChanged(ObservableCollection<ObservablePoint> oldValue, ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }

    public void Dispose()
    {
        _customTypeface?.Dispose();
        _telemetrySubscription.Dispose();
    }
}