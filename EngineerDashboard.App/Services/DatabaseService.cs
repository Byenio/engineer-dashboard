using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using EngineerDashboard.Database;
using EngineerDashboard.Database.Models;
using EngineerDashboard.Database.Services;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.Services;

public class DatabaseService : IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();
    
    private readonly AppDbContext _context;
    
    private int? _raceId { get; set; }
    private bool _isCreateRaceInProgress { get; set; } = false;

    public DatabaseService(AppDbContext context, TelemetryProvider telemetryProvider)
    {
        _context = context;
        
        _telemetrySubscription.Add(
            telemetryProvider.ParticipantsStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnParticipantsDataReceived));
        
        _telemetrySubscription.Add(
            telemetryProvider.SessionStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnSessionDataReceived));
    }

    private void OnParticipantsDataReceived(ParticipantsPacket packet)
    {
        
    }

    private void OnSessionDataReceived(SessionPacket packet)
    {
        var trackId = (int)packet.trackId;
        var aiDifficulty = packet.aiDifficulty;
        var raceLength = packet.totalLaps;

        var race = new Race
        {
            trackid = trackId,
            date = DateTime.UtcNow,
            aidifficulty = aiDifficulty,
            racelength = raceLength,
        };

        if (_raceId == null && !_isCreateRaceInProgress)
        {
            _isCreateRaceInProgress = true;
            Task.Run(async () =>
            {
                _raceId = (await RaceService.CreateRaceAsync(_context, race)).id;
                _isCreateRaceInProgress = false;
            });
        }
    }

    public void Dispose()
    {
        _telemetrySubscription.Dispose();
    }
}