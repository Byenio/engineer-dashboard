using System.Diagnostics;
using EngineerDashboard.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerDashboard.Database.Services;

public class RaceResultService
{
    public static async Task<RaceResult> CreateRaceResultAsync(AppDbContext context, RaceResult raceResult)
    {
        if (!await context.RaceEntries.AnyAsync(re => re.id == raceResult.race_entry_id))
            throw new InvalidOperationException("Race entry not found");
        
        context.RaceResults.Add(raceResult);
        await context.SaveChangesAsync();
        
        return raceResult;
    }
    
    public static async Task CreateRaceResultsAsync(AppDbContext context, IEnumerable<RaceResult> raceResults)
    {
        foreach (var raceResult in raceResults)
        {
            if (!await context.RaceEntries.AnyAsync(re => re.id == raceResult.race_entry_id))
                throw new InvalidOperationException($"Race entry not found");
        }

        context.RaceResults.AddRange(raceResults);

        await context.SaveChangesAsync();
    }
    
    public static async Task<int> GetDriverWinsCount(AppDbContext context, int driverId)
    {
        if (!await context.Drivers.AnyAsync(d => d.id == driverId))
            throw new InvalidOperationException("Driver not found");
        
        var wins = await context.RaceResults
            .Join(context.RaceEntries,
                rr => rr.race_entry_id,
                re => re.id,
                (rr, re) => new {RaceResult = rr, RaceEntry = re})
            .Where(joined => joined.RaceEntry.driver_id == driverId && joined.RaceResult.finish_position == 1)
            .CountAsync();
        
        Debug.WriteLine(wins);
        
        return wins;
    }
    
    public static async Task<int> GetDriverTopFinishesCount(AppDbContext context, int driverId, int position)
    {
        if (!await context.Drivers.AnyAsync(d => d.id == driverId))
            throw new InvalidOperationException("Driver not found");
        
        var finishes = await context.RaceResults
            .Join(context.RaceEntries,
                rr => rr.race_entry_id,
                re => re.id,
                (rr, re) => new {RaceResult = rr, RaceEntry = re})
            .Where(joined => joined.RaceEntry.driver_id == driverId && joined.RaceResult.finish_position <= position)
            .CountAsync();

        Debug.WriteLine(finishes);
        
        return finishes;
    }

    public static async Task<int> GetDriverPointsCount(AppDbContext context, int driverId)
    {
        if (!await context.Drivers.AnyAsync(d => d.id == driverId))
            throw new InvalidOperationException("Driver not found");

        var points = await context.RaceResults
            .Join(context.RaceEntries,
                rr => rr.race_entry_id,
                re => re.id,
                (rr, re) => new {RaceResult = rr, RaceEntry = re})
            .Where(joined => joined.RaceEntry.driver_id == driverId)
            .SumAsync(joined => joined.RaceResult.points);
        
        Debug.WriteLine(points);
        
        return points;
    }
}