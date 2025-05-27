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
    [ObservableProperty] private byte _drsAllowed;
    [ObservableProperty] private byte _drsOpen;
    [ObservableProperty] private ushort _deltaInFront;
    [ObservableProperty] private ushort _deltaLeader;
    [ObservableProperty] private VisualTyreCompound _visualTyreCompound;
    [ObservableProperty] private PitStatus _pitStatus;
    [ObservableProperty] private byte _tyreAgeLaps;
    [ObservableProperty] private byte _numPitStops;
    [ObservableProperty] private ushort _sector1Time;
    [ObservableProperty] private ushort _sector2Time;
    [ObservableProperty] private uint _lastLapTime;
    [ObservableProperty] private string _averageTyresWear;
    [ObservableProperty] private bool _isPlayer = false;

    [ObservableProperty] private bool _showTyreCompound = true;
    [ObservableProperty] private bool _showPit = false;
    
    public string CarPositionString => new string($"P{CarPosition}");
    public string DeltaInFrontString => TimeHelper.FormatMsToDeltaString(DeltaInFront);
    public string DeltaLeaderString => TimeHelper.FormatMsToDeltaString(DeltaLeader);
    public string VisualTyreCompoundString => new string($"({VisualTyreCompound.ToString()[..1]})");
    public string TyreAgeLapsString => new string($"{TyreAgeLaps} laps");
    public string Sector1TimeString => TimeHelper.FormatMsToSectorString(Sector1Time);
    public string Sector2TimeString => TimeHelper.FormatMsToSectorString(Sector2Time);
    public string LastLapTimeString => TimeHelper.FormatMsToLapTimeString(LastLapTime);

    public Brush TeamColor => TeamColorHelper.GetBrush(Team);
    public Brush DrsColor => DrsColorHelper.GetBrush(DrsAllowed, DrsOpen);
    public Brush TyreColor => TyreColorHelper.GetBrush(VisualTyreCompound);
    public Brush PlayerBorderColor => IsPlayer ? Brushes.Goldenrod : Brushes.Transparent;

    public DriversRowViewModel(int index)
    {
        Index = index;
        
        CarPosition = (byte)index + 1;
        DriverId = (byte)index;
        DriverName = "-";
        Team = Team.F1WORLD;
        DrsAllowed = 0;
        DrsOpen = 0;
        DeltaInFront = 0;
        DeltaLeader = 0;
        VisualTyreCompound = VisualTyreCompound.HARD;
        PitStatus = PitStatus.NONE;
        NumPitStops = 0;
        TyreAgeLaps = 0;
        Sector1Time = 0;
        Sector2Time = 0;
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
        DeltaInFront = data.deltaToCarInFrontInMS;
        DeltaLeader = data.deltaToRaceLeaderInMS;
        PitStatus = data.pitStatus;
        NumPitStops = data.numPitStops;
        Sector1Time = data.sector1TimeInMS != 0 ? data.sector1TimeInMS : Sector1Time;
        Sector2Time = data.sector2TimeInMS != 0 ? data.sector2TimeInMS : Sector2Time;
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

    partial void OnDrsAllowedChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(DrsColor));
    }

    partial void OnDrsOpenChanged(byte oldValue, byte newValue)
    {
        OnPropertyChanged(nameof(DrsColor));
    }

    partial void OnDeltaInFrontChanged(ushort oldValue, ushort newValue)
    {
        OnPropertyChanged(nameof(DeltaInFrontString));
    }

    partial void OnDeltaLeaderChanged(ushort oldValue, ushort newValue)
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

    partial void OnSector1TimeChanged(ushort oldValue, ushort newValue)
    {
        if (newValue == 0 && oldValue != 0)
            return;
        
        OnPropertyChanged(nameof(Sector1TimeString));
    }
    
    partial void OnSector2TimeChanged(ushort oldValue, ushort newValue)
    {
        if (newValue == 0 && oldValue != 0)
            return;
        
        OnPropertyChanged(nameof(Sector2TimeString));
    }

    partial void OnLastLapTimeChanged(uint oldValue, uint newValue)
    {
        OnPropertyChanged(nameof(LastLapTimeString));
    }

    partial void OnIsPlayerChanged(bool oldValue, bool newValue)
    {
        OnPropertyChanged(nameof(PlayerBorderColor));
    }
    
    #endregion
}