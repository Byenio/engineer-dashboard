using EngineerDashboard.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerDashboard.Database.Services;

public class RaceResultService
{
    public static async Task<RaceResult> CreateRaceResultAsync(AppDbContext context, RaceResult raceResult)
    {
        if (!await context.RaceEntries.AnyAsync(re => re.id == raceResult.raceentryid))
            throw new InvalidOperationException("Race entry not found");
        
        context.RaceResults.Add(raceResult);
        await context.SaveChangesAsync();
        
        return raceResult;
    }
    
    public static async Task CreateRaceResultsAsync(AppDbContext context, IEnumerable<RaceResult> raceResults)
    {
        foreach (var raceResult in raceResults)
        {
            if (!await context.RaceEntries.AnyAsync(re => re.id == raceResult.raceentryid))
                throw new InvalidOperationException($"Race entry not found");
        }

        context.RaceResults.AddRange(raceResults);

        await context.SaveChangesAsync();
    }
}