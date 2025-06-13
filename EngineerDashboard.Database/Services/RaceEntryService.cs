using System.Collections.ObjectModel;
using EngineerDashboard.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerDashboard.Database.Services;

public class RaceEntryService
{
    public static async Task<RaceEntry> CreateRaceEntryAsync(AppDbContext context, RaceEntry raceEntry)
    {
        if (!await context.Drivers.AnyAsync(d => d.id == raceEntry.driverid))
            throw new InvalidOperationException("Driver not found");
        
        if (!await context.Races.AnyAsync(r => r.id == raceEntry.raceid))
            throw new InvalidOperationException("Race not found");
        
        if (!await context.Teams.AnyAsync(t => t.id == raceEntry.teamid))
            throw new InvalidOperationException("Team not found");

        context.RaceEntries.Add(raceEntry);
        await context.SaveChangesAsync();

        return raceEntry;
    }

    public static async Task<List<RaceEntry>> GetRaceEntriesByDriverAsync(AppDbContext context, int driverId)
    {
        if (!await context.Drivers.AnyAsync(d => d.id == driverId))
            throw new InvalidOperationException("Driver not found");

        return await context.RaceEntries
            .Where(entry => entry.driverid == driverId)
            .Include(entry => entry.race)
            .Include(entry => entry.raceresult)
            .Include(entry => entry.race.track)
            .ToListAsync();
    }
}