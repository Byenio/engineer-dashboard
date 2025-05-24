using EngineerDashboard.Telemetry.Data;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.Services;

public class PartialTelemetryRow
{
    public float SessionTimestamp { get; set; }
    public DateTime LastUpdated { get; set; }
    
    public CarDamagePacket? CarDamagePacket { get; set; }
    public CarStatusPacket? CarStatusPacket { get; set; }
    public CarTelemetryPacket? CarTelemetryPacket { get; set; }
    public LapDataPacket? LapDataPacket { get; set; }
    public SessionPacket? SessionPacket { get; set; }
    
    public int? StartingPosition { get; set; }
    public int? FinishingPosition { get; set; }
    public bool? HasFastestLap { get; set; }
    public bool? HasDnf { get; set; }
    public int? PenaltiesTime { get; set; }

    public bool IsComplete => CarDamagePacket != null && CarStatusPacket != null && CarTelemetryPacket != null && LapDataPacket != null && SessionPacket != null;

    public string ToCSV()
    {
        if (!IsComplete) return string.Empty;

        var playerId = LapDataPacket?.header.playerCarIndex;
        
        if (playerId == null) return string.Empty;

        return parseDataToRow((int)playerId);
    }

    private string parseDataToRow(int playerId)
    {
        var carDamageData = CarDamagePacket?.carDamageData[playerId];
        var carStatusData = CarStatusPacket?.carStatusData[playerId];
        var carTelemetryData = CarTelemetryPacket?.carTelemetryData[playerId];
        var lapData = LapDataPacket?.lapData[playerId];
        var sessionData = SessionPacket;

        var carDamageString = string.Join(',',
            Math.Round((double)carDamageData?.tyresWear[0], 3),
            Math.Round((double)carDamageData?.tyresWear[1], 3),
            Math.Round((double)carDamageData?.tyresWear[2], 3),
            Math.Round((double)carDamageData?.tyresWear[3], 3)
            );

        var carStatusString = string.Join(',',
            Math.Round((double)carStatusData?.fuelInTank, 3),
            carStatusData?.actualTyreCompound,
            carStatusData?.visualTyreCompound,
            carStatusData?.tyresAgeLaps,
            carStatusData?.ersStoreEnergy
            );

        var carTelemetryString = string.Join(',',
            carTelemetryData?.tyresSurfaceTemperature[0],
            carTelemetryData?.tyresSurfaceTemperature[1],
            carTelemetryData?.tyresSurfaceTemperature[2],
            carTelemetryData?.tyresSurfaceTemperature[3],
            carTelemetryData?.tyresInnerTemperature[0],
            carTelemetryData?.tyresInnerTemperature[1],
            carTelemetryData?.tyresInnerTemperature[2],
            carTelemetryData?.tyresInnerTemperature[3],
            Math.Round((double)carTelemetryData?.tyresPressure[0], 3),
            Math.Round((double)carTelemetryData?.tyresPressure[1], 3),
            Math.Round((double)carTelemetryData?.tyresPressure[2], 3),
            Math.Round((double)carTelemetryData?.tyresPressure[3], 3)
            );

        var lapString = string.Join(',',
            Math.Round((double)lapData?.lapDistance, 3),
            lapData?.lastLapTimeInMS,
            lapData?.sector1TimeInMS,
            lapData?.sector2TimeInMS,
            lapData?.deltaToCarInFrontInMS,
            lapData?.deltaToRaceLeaderInMS,
            lapData?.carPosition,
            lapData?.currentLapNum,
            lapData?.numPitStops,
            lapData?.pitLaneTimerActive,
            lapData?.pitStopTimerInMS
            );

        var sessionString = string.Join(',',
            sessionData?.trackTemperature,
            sessionData?.airTemperature,
            sessionData?.totalLaps,
            sessionData?.pitStopRejoinPosition
            );

        return string.Join(',',
            SessionTimestamp,
            carDamageString,
            carStatusString,
            carTelemetryString,
            lapString,
            sessionString
            );
    }

    private static string _carDamageHeader => "tyre_wear_rear_left,tyre_wear_rear_right,tyre_wear_front_left,tyre_wear_front_right";
    private static string _carStatusHeader => "fuel_in_tank,actual_tyre_compound,visual_tyre_compound,tyres_age_laps,ers_store_energy";
    private static string _carTelemetryHeader =>
        "tyre_surface_temp_rear_left,tyre_surface_temp_rear_right,tyre_surface_temp_front_left,tyre_surface_temp_front_right," +
        "tyre_inner_temp_rear_left,tyre_inner_temp_rear_right,tyre_inner_temp_front_left,tyre_inner_temp_front_right," +
        "tyre_pressure_rear_left,tyre_pressure_rear_right,tyre_pressure_front_left,tyre_pressure_front_right";
    private static string _lapHeader =>
        "lap_distance,last_lap_ms,sector_1_ms,sector_2_ms,delta_in_front_ms,delta_leader_ms,car_position,current_lap,num_pit_stops,pit_timer_active,pit_timer_ms";
    private static string _sessionHeader => "track_temp,air_temp,total_laps,pit_rejoin_position";

    public static string Header =>
        string.Join(',', "Timestamp", _carDamageHeader, _carStatusHeader, _carTelemetryHeader, _lapHeader, _sessionHeader);
}