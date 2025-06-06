using EngineerDashboard.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerDashboard.Database;

public class AppDbContext : DbContext
{
    public DbSet<Team> Teams { get; set; }
    public DbSet<Rank> Ranks { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<Race> Races { get; set; }
    public DbSet<RaceEntry> RaceEntries { get; set; }
    public DbSet<Lap> Laps { get; set; }
    public DbSet<Stint> Stints { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("teams");
            entity.HasKey(e => e.id);
            entity.Property(e => e.id).ValueGeneratedNever();
            entity.Property(e => e.name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Rank>(entity =>
        {
            entity.ToTable("ranks");
            entity.HasKey(e => e.id);
            entity.Property(e => e.name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.icon).HasMaxLength(255);
            entity.Property(e => e.minpoints).IsRequired();
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.ToTable("drivers");
            entity.HasKey(e => e.id);
            entity.Property(e => e.id).ValueGeneratedNever();
            entity.Property(e => e.name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.elo).IsRequired().HasDefaultValue(null);
            entity.Property(e => e.rankid).IsRequired().HasDefaultValue(1);
            entity.HasOne(e => e.rank)
                .WithMany(r => r.drivers)
                .HasForeignKey(e => e.rankid)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.team)
                .WithMany(t => t.drivers)
                .HasForeignKey(e => e.teamid)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(e => e.raceentries)
                .WithOne(re => re.driver)
                .HasForeignKey(re => re.driverid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.ToTable("tracks");
            entity.HasKey(e => e.id);
            entity.Property(e => e.id).ValueGeneratedNever();
            entity.Property(e => e.name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Race>(entity =>
        {
            entity.ToTable("races");
            entity.HasKey(e => e.id);
            entity.Property(e => e.date).IsRequired();
            entity.Property(e => e.aidifficulty).IsRequired();
            entity.Property(e => e.racelength).IsRequired();
            entity.HasOne(e => e.track)
                .WithMany(t => t.races)
                .HasForeignKey(e => e.trackid)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.raceentries)
                .WithOne(re => re.race)
                .HasForeignKey(re => re.raceid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RaceEntry>(entity =>
        {
            entity.ToTable("raceentries");
            entity.HasKey(e => e.id);
            entity.Property(e => e.hasfastestlap).HasDefaultValue(false);
            entity.Property(e => e.hasdnf).HasDefaultValue(false);
            entity.HasOne(e => e.driver)
                .WithMany(d => d.raceentries)
                .HasForeignKey(e => e.driverid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.race)
                .WithMany(r => r.raceentries)
                .HasForeignKey(e => e.raceid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.team)
                .WithMany(t => t.raceentries)
                .HasForeignKey(e => e.teamid)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(e => e.laps)
                .WithOne(l => l.raceentry)
                .HasForeignKey(l => l.raceentryid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.stints)
                .WithOne(s => s.raceentry)
                .HasForeignKey(s => s.raceentryid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.driverid, e.raceid }).IsUnique();
        });

        modelBuilder.Entity<Lap>(entity =>
        {
            entity.ToTable("laps");
            entity.HasKey(e => e.id);
            entity.Property(e => e.lapnum).IsRequired();
            entity.Property(e => e.currentposition).IsRequired();
            entity.HasOne(e => e.raceentry)
                .WithMany(re => re.laps)
                .HasForeignKey(e => e.raceentryid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.raceentryid, e.lapnum }).IsUnique();
        });

        modelBuilder.Entity<Stint>(entity =>
        {
            entity.ToTable("stints");
            entity.HasKey(e => e.id);
            entity.Property(e => e.endlap).IsRequired();
            entity.HasOne(e => e.raceentry)
                .WithMany(re => re.stints)
                .HasForeignKey(e => e.raceentryid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.raceentryid, e.endlap }).IsUnique();
        });
    }
}