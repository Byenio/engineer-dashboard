using System.Collections.ObjectModel;
using System.Diagnostics;
using EngineerDashboard.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerDashboard.Database.Services;

public class LapService
{
    public static async Task<Lap> CreateLapAsync(AppDbContext context, Lap lap)
    {
        if (!await context.RaceEntries.AnyAsync(re => re.id == lap.race_entry_id))
            throw new InvalidOperationException("Race entry not found");

        context.Laps.Add(lap);
        await context.SaveChangesAsync();
        
        return lap;
    }

    public static async Task<Collection<Lap>> GetLaps(AppDbContext context, int raceEntryId)
    {
        if (!await context.RaceEntries.AnyAsync(re => re.id == raceEntryId))
            throw new InvalidOperationException("Race entry not found");
        
        var laps = await context.Laps
            .Where(l => l.race_entry_id == raceEntryId)
            .Include(l => l.tyre_compound)
            .AsNoTracking()
            .ToListAsync();
        return new Collection<Lap>(laps);
    }
}