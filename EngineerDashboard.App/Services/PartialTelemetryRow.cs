using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.Services;

public class PartialTelemetryRow
{
    public float SessionTimestamp { get; set; }
    public DateTime LastUpdated { get; set; }
    
    public LapDataPacket? LapDataPacket { get; set; }
    public CarTelemetryPacket? CarTelemetryPacket { get; set; }
    public SessionPacket? SessionPacket { get; set; }

    public bool IsComplete => LapDataPacket != null && CarTelemetryPacket != null && SessionPacket != null;

    public string ToCSV()
    {
        if (!IsComplete) return string.Empty;

        var playerId = LapDataPacket?.header.playerCarIndex;
        
        if (playerId == null) return string.Empty;
        
        var lapData = LapDataPacket?.lapData[(int)playerId];
        var carTelemetryData = CarTelemetryPacket?.carTelemetryData[(int)playerId];
        var sessionData = SessionPacket;

        return string.Join(",",
            SessionTimestamp,
            lapData?.lastLapTimeInMS,
            carTelemetryData?.speed,
            carTelemetryData?.throttle,
            carTelemetryData?.brake,
            carTelemetryData?.gear,
            sessionData?.airTemperature,
            sessionData?.trackTemperature
        );
    }
    
    public static string Header =>
        "Timestamp,LastLap,Speed,Throttle,Brake,Gear,AirTemp,TrackTemp";
}