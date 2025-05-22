using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.Telemetry;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.ViewModels;

public partial class DriversRowViewModel : ObservableObject
{
    public int Index { get; }

    [ObservableProperty] private byte _driverId;
    [ObservableProperty] private bool _isPlayer;
    [ObservableProperty] private string _name;
    [ObservableProperty] private Team _team;
    [ObservableProperty] private string _averageTyresWear;
    [ObservableProperty] private int _carPosition;

    public DriversRowViewModel(int index)
    {
        Index = index;
        
        DriverId = (byte)index;
        IsPlayer = true;
        Name = "-";
        Team = Team.F1WORLD;
        AverageTyresWear = "0%";
        CarPosition = (byte)index;
    }

    public void UpdateFromParticipantsPacket(ParticipantsPacket packet)
    {
        var data = packet.participants[Index];
        DriverId = data.driverId;
        IsPlayer = packet.header.playerCarIndex == Index;
        
        Name = Encoding.UTF8.GetString(data.name).TrimEnd('\0');
        
        Team = data.teamId;
    }

    public void UpdateFromLapDataPacket(LapDataPacket packet)
    {
        var data = packet.lapData[Index];
        CarPosition = data.carPosition;
    }

    public void UpdateFromCarDamagePacket(CarDamagePacket packet)
    {
        var data = packet.carDamageData[Index];
        AverageTyresWear = $"{data.tyresWear.Average():F0}%";
    }
}