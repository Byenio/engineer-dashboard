using System.Collections.ObjectModel;
using EngineerDashboard.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerDashboard.Database.Services;

public class RaceEntryService
{
    public static async Task<RaceEntry> CreateRaceEntryAsync(AppDbContext context, RaceEntry raceEntry)
    {
        if (!await context.Drivers.AnyAsync(d => d.id == raceEntry.driver_id))
            throw new InvalidOperationException("Driver not found");
        
        if (!await context.Races.AnyAsync(r => r.id == raceEntry.race_id))
            throw new InvalidOperationException("Race not found");

        context.RaceEntries.Add(raceEntry);
        await context.SaveChangesAsync();

        return raceEntry;
    }

    public static async Task<List<RaceEntry>> GetRaceEntriesByDriverAsync(AppDbContext context, int driverId)
    {
        if (!await context.Drivers.AnyAsync(d => d.id == driverId))
            throw new InvalidOperationException("Driver not found");

        return await context.RaceEntries
            .Where(entry => entry.driver_id == driverId)
            .Include(entry => entry.race)
            .Include(entry => entry.race_result)
            .Include(entry => entry.race.track)
            .OrderByDescending(re => re.race.date)
            .ToListAsync();
    }
    
    public static async Task<int> GetDriverRaceEntriesCount(AppDbContext context, int driverId)
    {
        if (!await context.Drivers.AnyAsync(d => d.id == driverId))
            throw new InvalidOperationException("Driver not found");

        var races = await context.RaceEntries
            .Where(entry => entry.driver_id == driverId)
            .CountAsync();
        
        return races;
    }
}