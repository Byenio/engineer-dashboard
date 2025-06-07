using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using EngineerDashboard.Database;
using EngineerDashboard.Database.Models;
using EngineerDashboard.Database.Services;
using EngineerDashboard.Telemetry;
using EngineerDashboard.Telemetry.Data;
using EngineerDashboard.Telemetry.Packets;
using Microsoft.EntityFrameworkCore;

namespace EngineerDashboard.App.Services;

public class DatabaseService : IDisposable
{
    private readonly CompositeDisposable _telemetrySubscription = new();
    
    private readonly AppDbContext _context;

    private Race _race;
    private List<Driver> _drivers = new List<Driver>();
    private List<byte> _startPositions = new List<byte>();
    private List<RaceEntry> _raceEntries = new List<RaceEntry>();
    
    private Dictionary<int, byte> _lastLoggedLapNums = new();
    private Dictionary<int, double> _latestTyreWear = new();
    private Dictionary<int, byte> _latestVisualTyreCompound = new();
    private Dictionary<int, ushort> _latestPitStopTime = new();
    private Dictionary<int, byte> _lastPitLimiterStatus = new();
    private Dictionary<int, double> _latestAverageDamage = new();

    private byte _currentFastestLapId;

    private int? _raceId = null;
    private int _numDrivers = 0;
    
    private bool _hasDrivers = false;
    private bool _hasRaceInfo = false;
    private bool _hasStartPositions = false;
    private bool _hasSavedToDatabase = false;

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

        _telemetrySubscription.Add(
            telemetryProvider.LapDataStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnLapDataReceived));
        
        _telemetrySubscription.Add(
            telemetryProvider.CarDamageStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnCarDamageDataReceived));
        
        _telemetrySubscription.Add(
            telemetryProvider.CarStatusStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnCarStatusDataReceived));
        
        _telemetrySubscription.Add(
            telemetryProvider.EventStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnEventReceived));

        _telemetrySubscription.Add(
            telemetryProvider.FinalClassificationStream
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnFinalClassificationDataReceived));
    }
    
    #region telemetry handlers

    private void OnParticipantsDataReceived(ParticipantsPacket packet)
    {
        if (_hasDrivers) return;

        foreach (var driver in packet.participants)
        {
            var id = driver.driverId;
            var name = Encoding.UTF8.GetString(driver.name).TrimEnd('\0');
            var teamid = (int)driver.teamId;

            var newDriver = new Driver
            {
                id = id,
                name = name,
                teamid = teamid,
            };

            _drivers.Add(newDriver);
        }

        _hasDrivers = true;
        _numDrivers = packet.numActiveCars;
        TrySaveToDatabase();
    }
    
    private void OnCarDamageDataReceived(CarDamagePacket packet)
    {
        for (int i = 0; i < _numDrivers; i++)
        {
            var data = packet.carDamageData[i];
            
            _latestTyreWear[i] = Math.Round(data.tyresWear.Max(), 0);
            double totalDamage = data.frontLeftWingDamage +
                                 data.frontRightWingDamage +
                                 data.rearWingDamage +
                                 data.diffuserDamage +
                                 data.floorDamage +
                                 data.sidepodDamage;
            _latestAverageDamage[i] = Math.Round(totalDamage / 6, 0);
        }
    }


    private void OnSessionDataReceived(SessionPacket packet)
    {
        if (_hasRaceInfo) return;

        _race = new Race
        {
            trackid = (int)packet.trackId,
            date = DateTime.UtcNow,
            aidifficulty = packet.aiDifficulty,
            racelength = packet.totalLaps,
        };

        _hasRaceInfo = true;
        TrySaveToDatabase();
    }

    private void OnLapDataReceived(LapDataPacket packet)
    {
        if (!_hasStartPositions)
        {
            foreach (var entry in packet.lapData)
            {
                _startPositions.Add(entry.carPosition);
            }

            _hasStartPositions = true;
            TrySaveToDatabase();
        }

        for (int i = 0; i < _numDrivers; i++)
        {
            var lapData = packet.lapData[i];
            var currentLapNum = lapData.currentLapNum;

            if (lapData.pitStopTimerInMS != 0)
            {
                _latestPitStopTime[i] = lapData.pitStopTimerInMS;
            }

            if (currentLapNum < 2)
                continue;

            if (_lastLoggedLapNums.TryGetValue(i, out var lastLoggedLap) &&
                lastLoggedLap == currentLapNum)
                continue;

            var lap = new Lap
            {
                raceentryid = _raceEntries[i].id,
                lapnum = lapData.currentLapNum - 1,
                currentposition = lapData.carPosition,
                deltatoleader = lapData.deltaToRaceLeaderInMS,
                deltatocarinfront = lapData.deltaToCarInFrontInMS,
                lastlaptime = (int)lapData.lastLapTimeInMS,
                tyrewear = (int)_latestTyreWear[i],
            };

            Task.Run(async () =>
            {
                await LapService.CreateLapAsync(_context, lap);
            });
            
            _lastLoggedLapNums[i] = currentLapNum;
        }
    }

    private void OnCarStatusDataReceived(CarStatusPacket packet)
    {
        for (int i = 0; i < _numDrivers; i++)
        {
            var carStatusData = packet.carStatusData[i];
            var currentPitLimiterStatus = carStatusData.pitLimiterStatus;

            _lastPitLimiterStatus.TryGetValue(i, out var lastPitLimiterStatus);

            if (lastPitLimiterStatus == 0 && currentPitLimiterStatus == 0)
            {
                _latestVisualTyreCompound[i] = (byte)carStatusData.visualTyreCompound;
                _lastPitLimiterStatus[i] = currentPitLimiterStatus;
                continue;
            }

            if (lastPitLimiterStatus == currentPitLimiterStatus ||
                (lastPitLimiterStatus == 0 && currentPitLimiterStatus == 1) ||
                !_latestVisualTyreCompound.ContainsKey(i) ||
                !_latestTyreWear.ContainsKey(i) ||
                !_latestPitStopTime.ContainsKey(i))
            {
                _lastPitLimiterStatus[i] = currentPitLimiterStatus;
                continue;
            }

            var stint = new Stint
            {
                raceentryid = _raceEntries[i].id,
                endlap = _lastLoggedLapNums[i],
                tyrecompound = _latestVisualTyreCompound[i],
                tyrewear = (int)_latestTyreWear[i],
                pitstoptime = _latestPitStopTime[i]
            };

            _lastPitLimiterStatus[i] = currentPitLimiterStatus;

            Task.Run(async () =>
            {
                await StintService.CreateStintAsync(_context, stint);
            });
        }
    }
    
    private void OnEventReceived(EventPacket packet)
    {
        var stringCode = new string(packet.eventStringCode).TrimEnd('\0');
    
        if (stringCode == "SSTA")
        {
            ResetState();
        }

        if (stringCode == "FTLP")
        {
            _currentFastestLapId = packet.eventDetails.fastestLap.vehicleIdx;
        }
    }

    private void OnFinalClassificationDataReceived(FinalClassificationPacket packet)
    {
        Task.Run(async () =>
        {
            var raceResults = new List<RaceResult>();
            for (int i = 0; i < _numDrivers; i++)
            {
                var data = packet.classificationData[i];

                var raceResult = new RaceResult
                {
                    raceentryid = _raceEntries[i].id,
                    finishposition = data.position,
                    hasfastestlap = _currentFastestLapId == i,
                    penaltiesinseconds = data.penaltiesTime,
                    hasdnf = data.resultStatus != ResultStatus.FINISHED,
                    points = data.points,
                    averagedamage = (int)_latestAverageDamage[i]
                };

                raceResults.Add(raceResult);
            }

            await RaceResultService.CreateRaceResultsAsync(_context, raceResults);
        });
    }
    
    private void ResetState()
    {
        _drivers.Clear();
        _startPositions.Clear();
        _raceEntries.Clear();

        _lastLoggedLapNums.Clear();
        _latestTyreWear.Clear();
        _latestVisualTyreCompound.Clear();
        _latestPitStopTime.Clear();
        _latestAverageDamage.Clear();
        _lastPitLimiterStatus.Clear();

        _race = null;
        _raceId = null;
        _numDrivers = 0;
        _currentFastestLapId = 0;

        _hasDrivers = false;
        _hasRaceInfo = false;
        _hasStartPositions = false;
        _hasSavedToDatabase = false;
    }
    
    private void TrySaveToDatabase()
    {
        if (_hasSavedToDatabase) return;
        if (!_hasDrivers || !_hasRaceInfo || !_hasStartPositions) return;

        _hasSavedToDatabase = true;

        Task.Run(async () =>
        {
            for (int i = 0; i < _numDrivers; i++)
            {
                _drivers[i].id = (await DriverService.CreateDriverAsync(_context, _drivers[i])).id;
            }

            _raceId = (await RaceService.CreateRaceAsync(_context, _race)).id;

            for (int i = 0; i < _drivers.Count; i++)
            {
                var raceEntry = new RaceEntry
                {
                    driverid = _drivers[i].id,
                    raceid = _raceId.Value,
                    teamid = (int)_drivers[i].teamid,
                    startposition = _startPositions[i]
                };

                _raceEntries.Add(raceEntry);
                _raceEntries[i].id = (await RaceEntryService.CreateRaceEntryAsync(_context, raceEntry)).id;
            }
        });
    }
    
    #endregion
    
    #region queries

    public async Task<Collection<Driver>> GetDrivers()
    {
        var drivers = await DriverService.GetDrivers(_context);

        return drivers;
    } 
    
    #endregion
    
    public void Dispose()
    {
        _telemetrySubscription.Dispose();
    }
}