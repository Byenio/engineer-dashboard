using EngineerDashboard.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerDashboard.Database.Services;

public class StintService
{
    public static async Task<Stint> CreateStintAsync(AppDbContext context, Stint stint)
    {
        if (!await context.RaceEntries.AnyAsync(re => re.id == stint.raceentryid))
            throw new InvalidOperationException("Race entry not found");

        context.Stints.Add(stint);
        await context.SaveChangesAsync();
        
        return stint;
    }
}