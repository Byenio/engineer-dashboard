using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EngineerDashboard.App.Services;

public class TelemetryLoggerService : IDisposable
{
    private readonly ConcurrentDictionary<float, PartialTelemetryRow> _rows = new();
    private readonly Lock _fileLock = new();
    private readonly Timer _flushTimer;

    private bool _loggingActive = false;
    private bool _headerWritten = false;
    private string? _logFilePath;

    public TelemetryLoggerService(TelemetryProvider telemetryProvider)
    {
        telemetryProvider.LapDataStream
            .Subscribe(packet => OnPacket(packet.header.sessionTime, row => row.LapDataPacket = packet));
        
        telemetryProvider.CarTelemetryStream
            .Subscribe(packet => OnPacket(packet.header.sessionTime, row => row.CarTelemetryPacket = packet));
        
        telemetryProvider.SessionStream
            .Subscribe(packet => OnPacket(packet.header.sessionTime, row => row.SessionPacket = packet));

        telemetryProvider.EventStream
            .Subscribe(packet => HandleEvent(packet.eventStringCode));
        
        _flushTimer = new Timer(_ => FlushCompletedRows(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private void OnPacket(float sessionTime, Action<PartialTelemetryRow> apply)
    {
        if (!_loggingActive) return;
        
        var row = _rows.GetOrAdd(sessionTime, _ => new PartialTelemetryRow { SessionTimestamp = sessionTime });
        apply(row);
        row.LastUpdated = DateTime.UtcNow;
    }

    private void HandleEvent(char[] eventStringCode)
    {
        string stringCode = new string(eventStringCode);
        
        switch (stringCode)
        {
            case "SSTA":
                StartLogging();
                break;
            case "SEND":
                StopLogging();
                break;
        }
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
        
        _loggingActive = false;
        _logFilePath = null;
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
            Console.WriteLine(completed.Count);
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