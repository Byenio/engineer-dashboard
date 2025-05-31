using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Services;
using EngineerDashboard.Telemetry.Packets;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EngineerDashboard.App.ViewModels;

public partial class FuelChartViewModel : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();
    private readonly SKTypeface _customTypeface;
    
    [ObservableProperty] private ObservableCollection<ObservablePoint> _fuel;
    [ObservableProperty] private ISeries[] _series;
    [ObservableProperty] private byte _numLaps = 0;
    [ObservableProperty] private bool _received = false;
    
    public Axis[] XAxes =>
    [
        new Axis
        {
            Name = "Fuel usage",
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
            Labeler = value => $"{value:F0}kg",
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

    public FuelChartViewModel(TelemetryProvider telemetryProvider)
    {
        _customTypeface = LoadCustomFont();
        
        Fuel = new ObservableCollection<ObservablePoint>();
        
        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = Fuel,
                Name = "Fuel",
                GeometrySize = 6,
                Stroke = new SolidColorPaint(new SKColor(34, 139, 34), 2),
                GeometryStroke = new SolidColorPaint(new SKColor(34, 139, 34), 2),
                Fill = null,
                YToolTipLabelFormatter = point => $"Lap {point.Coordinate.SecondaryValue:F0}: {Math.Round(point.Coordinate.PrimaryValue, 2)}kg"
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
            telemetryProvider.CarStatusStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnCarStatusDataReceived));
    }

    private void OnLapDataReceived(LapDataPacket packet)
    {
        var playerId = packet.header.playerCarIndex;

        if (NumLaps > packet.lapData[playerId].currentLapNum)
        {
            Fuel.Clear();
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
            Fuel.Add(new ObservablePoint(NumLaps - 1, Math.Round(data.fuelInTank, 2)));
            
            Received = true;
        }

    }

    partial void OnFuelChanged(ObservableCollection<ObservablePoint> oldValue, ObservableCollection<ObservablePoint> newValue)
    {
        OnPropertyChanged(nameof(Series));
    }

    public void Dispose()
    {
        _customTypeface?.Dispose();
        _telemetrySubscription.Dispose();
    }
}