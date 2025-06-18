using System.Collections.ObjectModel;
using EngineerDashboard.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerDashboard.Database.Services;

public class PitStopService
{
    public static async Task<PitStop> CreatePitStopAsync(AppDbContext context, PitStop pitStop)
    {
        if (!await context.RaceEntries.AnyAsync(re => re.id == pitStop.race_entry_id))
            throw new InvalidOperationException("Race entry not found");

        context.PitStops.Add(pitStop);
        await context.SaveChangesAsync();
        
        return pitStop;
    }
    
    public static async Task<Collection<PitStop>> GetPitStops(AppDbContext context, int raceEntryId)
    {
        if (!await context.RaceEntries.AnyAsync(re => re.id == raceEntryId))
            throw new InvalidOperationException("Race entry not found");
        
        var pitStops = await context.PitStops
            .Where(p => p.race_entry_id == raceEntryId)
            .AsNoTracking()
            .ToListAsync();
    
        return new Collection<PitStop>(pitStops);
    }
}