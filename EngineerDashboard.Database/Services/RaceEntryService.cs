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
}