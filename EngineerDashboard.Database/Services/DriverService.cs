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

        if (!await context.Ranks.AnyAsync(r => r.id == driver.rankid))
            throw new InvalidOperationException("Rank not found.");
        
        if (driver.teamid != null && !await context.Teams.AnyAsync(t => t.id == driver.teamid))
            throw new InvalidOperationException("Team not found.");
        
        context.Drivers.Add(driver);
        await context.SaveChangesAsync();
        
        return driver;
    }
}