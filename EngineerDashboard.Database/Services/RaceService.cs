using System.Diagnostics;
using EngineerDashboard.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerDashboard.Database.Services;

public static class RaceService
{
    public static async Task<Race> CreateRaceAsync(AppDbContext context, Race race)
    {
        if (!await context.Tracks.AnyAsync(t => t.id == race.trackid))
            throw new InvalidOperationException($"Track not found.");
        
        context.Races.Add(race);
        await context.SaveChangesAsync();
        
        return race;
    }
}