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
}