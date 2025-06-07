using EngineerDashboard.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerDashboard.Database.Services;

public class LapService
{
    public static async Task<Lap> CreateLapAsync(AppDbContext context, Lap lap)
    {
        if (!await context.RaceEntries.AnyAsync(re => re.id == lap.raceentryid))
            throw new InvalidOperationException("Race entry not found");

        context.Laps.Add(lap);
        await context.SaveChangesAsync();
        
        return lap;
    }
}