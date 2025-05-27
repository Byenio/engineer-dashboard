using System.Globalization;
using System.Text;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using EngineerDashboard.App.Helpers;
using EngineerDashboard.Telemetry;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.ViewModels;

public partial class DriversRowViewModel : ObservableObject
{
    public int Index { get; }

    [ObservableProperty] private int _carPosition;
    [ObservableProperty] private byte _driverId;
    [ObservableProperty] private string _driverName;
    [ObservableProperty] private Team _team;
    [ObservableProperty] private bool _drsAllowed;
    [ObservableProperty] private bool _drsOpen;
    [ObservableProperty] private double _deltaInFront;
    [ObservableProperty] private double _deltaLeader;
    [ObservableProperty] private VisualTyreCompound _visualTyreCompound;
    [ObservableProperty] private PitStatus _pitStatus;
    [ObservableProperty] private byte _tyreAgeLaps;
    [ObservableProperty] private uint _lastLapTime;
    
    [ObservableProperty] private bool _isPlayer;
    [ObservableProperty] private string _averageTyresWear;

    [ObservableProperty] private bool _showTyreCompound = true;
    [ObservableProperty] private bool _showPit = false;
    
    public string CarPositionString => new string($"P{CarPosition}");
    public string DeltaInFrontString => new string($"+{DeltaInFront}");
    public string DeltaLeaderString => new string($"+{DeltaLeader}");
    public string VisualTyreCompoundString => new string($"({VisualTyreCompound.ToString()[..1]})");
    public string TyreAgeLapsString => new string($"{TyreAgeLaps} laps");
    public string LastLapTimeString => TimeHelper.FormatMsToString(LastLapTime);

    public Brush TeamColor => TeamColorHelper.GetBrush(Team);
    public Brush DrsColor => DrsColorHelper.GetBrush(DrsAllowed, DrsOpen);
    public Brush TyreColor => TyreColorHelper.GetBrush(VisualTyreCompound);

    public DriversRowViewModel(int index)
    {
        Index = index;
        
        CarPosition = (byte)index + 1;
        DriverId = (byte)index;
        IsPlayer = true;
        DriverName = "-";
        Team = Team.F1WORLD;
        DrsAllowed = false;
        DrsOpen = false;
        DeltaInFront = 0.000;
        DeltaLeader = 0.000;
        VisualTyreCompound = VisualTyreCompound.HARD;
        PitStatus = PitStatus.NONE;
        TyreAgeLaps = 0;
        LastLapTime = 0;
        
        AverageTyresWear = "0%";
    }
    
    #region UpdateFromPacket handlers

    public void UpdateFromParticipantsPacket(ParticipantsPacket packet)
    {
        var data = packet.participants[Index];
        DriverId = data.driverId;
        IsPlayer = packet.header.playerCarIndex == Index;
        DriverName = Encoding.UTF8.GetString(data.name).TrimEnd('\0');
        Team = data.teamId;
    }

    public void UpdateFromLapDataPacket(LapDataPacket packet)
    {
        var data = packet.lapData[Index];
        CarPosition = data.carPosition;
        DeltaInFront = Math.Round((double)data.deltaToCarInFrontInMS * 1000, 3);
        DeltaLeader = Math.Round((double)data.deltaToRaceLeaderInMS * 1000, 3);
        PitStatus = data.pitStatus;
        LastLapTime = data.lastLapTimeInMS;
    }

    public void UpdateFromCarDamagePacket(CarDamagePacket packet)
    {
        var data = packet.carDamageData[Index];
        AverageTyresWear = $"{data.tyresWear.Average():F0}%";
    }

    public void UpdateFromCarStatusPacket(CarStatusPacket packet)
    {
        var data = packet.carStatusData[Index];
        DrsAllowed = data.drsAllowed;
        VisualTyreCompound = data.visualTyreCompound;
        TyreAgeLaps = data.tyresAgeLaps;
    }

    public void UpdateFromCarTelemetryPacket(CarTelemetryPacket packet)
    {
        var data = packet.carTelemetryData[Index];
        DrsOpen = data.drs;
    }
    
    #endregion

    #region OnPropertyChanged handlers
    
    partial void OnCarPositionChanged(int oldValue, int newValue)
    {
        OnPropertyChanged(nameof(CarPositionString));
    }

    partial void OnTeamChanged(Team oldValue, Team newValue)
    {
        OnPropertyChanged(nameof(TeamColor));
    }

    partial void OnDrsAllowedChanged(bool oldValue, bool newValue)
    {
        OnPropertyChanged(nameof(DrsColor));
    }

    partial void OnDrsOpenChanged(bool oldValue, bool newValue)
    {
        OnPropertyChanged(nameof(DrsColor));
    }

    partial void OnDeltaInFrontChanged(double oldValue, double newValue)
    {
        OnPropertyChanged(nameof(DeltaInFrontString));
    }

    partial void OnDeltaLeaderChanged(double oldValue, double newValue)
    {
        OnPropertyChanged(nameof(DeltaLeaderString));
    }

    partial void OnVisualTyreCompoundChanged(VisualTyreCompound oldValue, VisualTyreCompound newValue)
    {
        OnPropertyChanged(nameof(VisualTyreCompoundString));
        OnPropertyChanged(nameof(TyreColor));
    }

    partial void OnPitStatusChanged(PitStatus oldValue, PitStatus newValue)
    {
        ShowPit = newValue != PitStatus.NONE;
        ShowTyreCompound = !ShowPit;
    }
    
    partial void OnTyreAgeLapsChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(TyreAgeLapsString));
    }

    partial void OnLastLapTimeChanged(uint oldValue, uint newValue)
    {
        OnPropertyChanged(nameof(LastLapTimeString));
    }
    
    #endregion
}