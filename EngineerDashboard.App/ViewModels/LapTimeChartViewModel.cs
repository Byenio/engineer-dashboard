using System;
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

public partial class LapTimeChartViewModel : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();
    
    [ObservableProperty] private ObservableCollection<ObservablePoint> _lapTimes;
    [ObservableProperty] private ISeries[] _series;
    [ObservableProperty] private byte _numLaps;
    
    public Axis[] XAxes =>
    [
        new Axis
        {
            Name = "Lap Number",
            MinStep = 1,
            Labeler = value => $"{value:F0}",
            LabelsPaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                // SKTypeface = SKTypeface.FromFile("../Assets/Fonts/JetBrainsMono-Bold.ttf")
            },
            NamePaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                // SKTypeface = SKTypeface.FromFile("Assets/Fonts/JetBrainsMono-Bold.ttf")
            }
        }
    ];

    public Axis[] YAxes =>
    [
        new Axis
        {
            Labeler = value => Formatter.FormatMsToLapTimeString((uint)value),
            LabelsPaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                // SKTypeface = SKTypeface.FromFile("Assets/Fonts/JetBrainsMono-Bold.ttf")
            },
            NamePaint = new SolidColorPaint
            {
                Color = new SKColor(230, 230, 230),
                // SKTypeface = SKTypeface.FromFile("Assets/Fonts/JetBrainsMono-Bold.ttf")
            },
            SeparatorsPaint = new SolidColorPaint
            {
                Color = new SKColor(48, 48, 48)
            }
        }
    ];

    public LapTimeChartViewModel(TelemetryProvider telemetryProvider)
    {
        LapTimes = new ObservableCollection<ObservablePoint>();
        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = LapTimes,
                Name = "Player Lap Times",
                GeometrySize = 6,
                Stroke = new SolidColorPaint(new SKColor(255, 155, 0), 2),
                GeometryStroke = new SolidColorPaint(new SKColor(255, 155, 0), 2),
                Fill = null,
                YToolTipLabelFormatter = point => $"Lap {point.Coordinate.SecondaryValue:F0}: {Formatter.FormatMsToLapTimeString((uint)point.Coordinate.PrimaryValue)}"
            }
        };

        HookEvents(telemetryProvider);
    }

    private void HookEvents(TelemetryProvider telemetryProvider)
    {
        _telemetrySubscription.Add(
            telemetryProvider.SessionHistoryStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnSessionHistoryDataReceived));
    }

    private void OnSessionHistoryDataReceived(SessionHistoryPacket packet)
    {
        var playerId = packet.header.playerCarIndex;

        if (playerId != packet.carIdx) return;

        if (NumLaps == packet.numLaps) return;

        if (NumLaps < packet.numLaps - 1 || packet.numLaps == 0)
        {
            LapTimes.Clear();
            for (int i = 0; i < packet.numLaps; i++)
            {
                var lapData = packet.lapHistoryData[i];
                if (lapData.lapTimeInMS != 0)
                {
                    LapTimes.Add(new ObservablePoint(i + 1, lapData.lapTimeInMS));
                }
            }
            NumLaps = packet.numLaps;
        }
        else if (packet.numLaps > 1)
        {
            var latestLap = packet.lapHistoryData[packet.numLaps - 2];
            
            if (latestLap.lapTimeInMS != 0)
            {
                LapTimes.Add(new ObservablePoint(packet.numLaps - 1, latestLap.lapTimeInMS));
                NumLaps = packet.numLaps;
            }
        }

    }

    partial void OnLapTimesChanged(ObservableCollection<ObservablePoint> oldValue, ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }

    public void Dispose()
    {
        _telemetrySubscription.Dispose();
    }
}