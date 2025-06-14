using System.Collections.ObjectModel;
using EngineerDashboard.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerDashboard.Database.Services;

public class DriverService
{
    public static async Task<Driver> CreateDriverAsync(AppDbContext context, Driver driver)
    {
        if (await context.Drivers.AnyAsync(d => d.id == driver.id))
        {
            var result = context.Drivers.FindAsync(driver.id).Result;
            if (result != null)
                return result;
        }

        if (!await context.Ranks.AnyAsync(r => r.id == driver.rank_id))
            throw new InvalidOperationException("Rank not found.");
        
        if (driver.team_id != null && !await context.Teams.AnyAsync(t => t.id == driver.team_id))
            throw new InvalidOperationException("Team not found.");
        
        context.Drivers.Add(driver);
        await context.SaveChangesAsync();
        
        return driver;
    }

    public static async Task<Collection<Driver>> GetDrivers(AppDbContext context)
    {
        var drivers = await context.Drivers
            .Include(d => d.rank)
            .Include(d => d.team)
            .Include(d => d.race_entries)
            .AsNoTracking()
            .ToListAsync();
        
        return new Collection<Driver>(drivers);
    }
}