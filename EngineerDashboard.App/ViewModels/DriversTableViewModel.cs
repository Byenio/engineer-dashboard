using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Services;
using EngineerDashboard.Telemetry;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.ViewModels;

public partial class DriversTableViewModel : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();
    
    public ObservableCollection<DriversRowViewModel> Drivers { get; } = new();

    public DriversTableViewModel(TelemetryProvider telemetryProvider)
    {
        for (int i = 0; i < 20; i++)
        {
            Drivers.Add(new DriversRowViewModel(i));
        }
        
        HookEvents(telemetryProvider, SynchronizationContext.Current!);
    }

    private void HookEvents(TelemetryProvider telemetryProvider, SynchronizationContext uiContext)
    {
        _telemetrySubscription.Add(
            telemetryProvider.ParticipantsStream
                .ObserveOn(uiContext)
                .Subscribe(OnParticipantsDataReceive)
        );
        
        _telemetrySubscription.Add(
            telemetryProvider.CarDamageStream
                .ObserveOn(uiContext)
                .Subscribe(OnCarDamageDataReceive)
        );
        
        _telemetrySubscription.Add(
            telemetryProvider.LapDataStream
                .ObserveOn(uiContext)
                .Subscribe(OnLapDataReceive)
        );
    }

    private void OnParticipantsDataReceive(ParticipantsPacket packet)
    {
        foreach (var driver in Drivers)
        {
            driver.UpdateFromParticipantsPacket(packet);
        }
    }

    private void OnCarDamageDataReceive(CarDamagePacket packet)
    {
        foreach (var driver in Drivers)
        {
            driver.UpdateFromCarDamagePacket(packet);
        }
    }

    private void OnLapDataReceive(LapDataPacket packet)
    {
        foreach (var driver in Drivers)
        {
            driver.UpdateFromLapDataPacket(packet);
        }

        SortByPosition();
    }

    public void SortByPosition()
    {
        var sorted = Drivers.OrderBy(d => d.CarPosition).ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            var currentIndex = Drivers.IndexOf(sorted[i]);
            if (currentIndex != i)
            {
                Drivers.Move(currentIndex, i);
            }
        }
    }

    public void Dispose()
    {
        _telemetrySubscription.Dispose();
    }
}