using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using EngineerDashboard.Telemetry;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.Services;

public class TelemetryLoggerService : IDisposable
{
    private readonly ConcurrentDictionary<float, PartialTelemetryRow> _rows = new();
    private readonly Lock _fileLock = new();
    private readonly Timer _flushTimer;

    private bool _loggingActive = false;
    private bool _headerWritten = false;
    private string? _logFilePath;

    private byte _fastestLapDriverId;

    public TelemetryLoggerService(TelemetryProvider telemetryProvider)
    {
        telemetryProvider.CarDamageStream
            .Subscribe(packet => OnPacket(packet.header.sessionTime, row => row.CarDamagePacket = packet));
        
        telemetryProvider.CarStatusStream
            .Subscribe(packet => OnPacket(packet.header.sessionTime, row => row.CarStatusPacket = packet));
        
        telemetryProvider.CarTelemetryStream
            .Subscribe(packet => OnPacket(packet.header.sessionTime, row => row.CarTelemetryPacket = packet));

        telemetryProvider.LapDataStream
            .Subscribe(packet => OnPacket(packet.header.sessionTime, row => row.LapDataPacket = packet));
        
        telemetryProvider.SessionStream
            .Subscribe(packet => OnPacket(packet.header.sessionTime, row => row.SessionPacket = packet));

        telemetryProvider.EventStream
            .Subscribe(HandleEvent);

        telemetryProvider.FinalClassificationStream
            .Subscribe(HandleFinalClassification);
        
        _flushTimer = new Timer(_ => FlushCompletedRows(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private void OnPacket(float sessionTime, Action<PartialTelemetryRow> apply)
    {
        if (!_loggingActive) return;
        
        var row = _rows.GetOrAdd(sessionTime, _ => new PartialTelemetryRow { SessionTimestamp = sessionTime });
        apply(row);
        row.LastUpdated = DateTime.UtcNow;
    }

    private void HandleEvent(EventPacket packet)
    {
        var stringCode = new string(packet.eventStringCode);
        
        switch (stringCode)
        {
            case "SSTA":
                StartLogging();
                break;
            case "SEND":
                StopLogging();
                break;
            case "FTLP":
                _fastestLapDriverId = packet.eventDetails.fastestLap.vehicleIdx;
                break;
        }
    }

    private void HandleFinalClassification(FinalClassificationPacket packet)
    {
        var playerId = packet.header.playerCarIndex;

        var data = packet.classificationData[(int)playerId];

        var lines = File.ReadAllLines(_logFilePath).ToList();
        
        lines[0] += ",starting_position,finishing_position,fastest_lap,dnf,penalties_time";

        var startingPosition = data.gridPosition;
        var finishingPosition = data.position;
        var hasDnf = data.resultStatus != ResultStatus.FINISHED;
        var penaltiesTime = data.penaltiesTime;
        var hasFastestLap = _fastestLapDriverId == playerId;

        for (int i = 1; i < lines.Count; i++)
        {
            lines[i] += $",{startingPosition},{finishingPosition},{hasDnf},{penaltiesTime},{hasFastestLap}";
        }
        
        File.WriteAllLines(_logFilePath, lines);
        
        _loggingActive = false;
        _logFilePath = null;
    }

    private void StartLogging()
    {
        if (_loggingActive) return;
        
        string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(logDirectory);

        _logFilePath = Path.Combine(logDirectory, $"{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.csv");
        
        _headerWritten = false;
        _loggingActive = true;
        
        _flushTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void StopLogging()
    {
        if (!_loggingActive) return;
        
        _flushTimer.Change(Timeout.Infinite, Timeout.Infinite);
        FlushCompletedRows();
    }
    
    private void FlushCompletedRows()
    {
        if (!_loggingActive || string.IsNullOrEmpty(_logFilePath)) return;
        
        var now = DateTime.UtcNow;
        
        var completed = _rows
            .Where(kvp => kvp.Value.IsComplete && now - kvp.Value.LastUpdated > TimeSpan.FromSeconds(1))
            .OrderBy(kvp => kvp.Key)
            .ToList();

        foreach (var kvp in completed)
        {
            WriteRowToFile(kvp.Value);
            _rows.TryRemove(kvp.Key, out _);
        }
    }

    private void WriteRowToFile(PartialTelemetryRow row)
    {
        if (string.IsNullOrEmpty(_logFilePath)) return;
        
        var csvRow = row.ToCSV();
        if (string.IsNullOrWhiteSpace(csvRow)) return;

        lock (_fileLock)
        {
            using var writer = new StreamWriter(_logFilePath, append: true, Encoding.UTF8);
            
            if (!_headerWritten)
            {
                writer.WriteLine(PartialTelemetryRow.Header);
                _headerWritten = true;
            }
            
            writer.WriteLine(csvRow);
        }
    }
    
    public void Dispose()
    {
        _flushTimer.Dispose();
        FlushCompletedRows();
    }
}