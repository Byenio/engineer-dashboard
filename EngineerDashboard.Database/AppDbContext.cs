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
    public DbSet<RaceResult> RaceResults { get; set; }
    public DbSet<TyreCompound> TyreCompounds { get; set; }
    public DbSet<Lap> Laps { get; set; }
    public DbSet<PitStop> PitStops { get; set; }

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
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.icon).HasMaxLength(255);
            entity.Property(e => e.min_points).IsRequired();
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.ToTable("drivers");
            entity.HasKey(e => e.id);
            entity.Property(e => e.id).ValueGeneratedNever();
            entity.Property(e => e.name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.elo).IsRequired().HasDefaultValue(1000);
            entity.Property(e => e.rank_id).IsRequired().HasDefaultValue(1);
            entity.HasOne(e => e.rank)
                .WithMany(r => r.drivers)
                .HasForeignKey(e => e.rank_id)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.team)
                .WithMany(t => t.drivers)
                .HasForeignKey(e => e.team_id)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(e => e.race_entries)
                .WithOne(re => re.driver)
                .HasForeignKey(re => re.driver_id)
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
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.date).IsRequired();
            entity.Property(e => e.ai_difficulty).IsRequired();
            entity.Property(e => e.length).IsRequired();
            entity.HasOne(e => e.track)
                .WithMany(t => t.races)
                .HasForeignKey(e => e.track_id)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.race_entries)
                .WithOne(re => re.race)
                .HasForeignKey(re => re.race_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RaceEntry>(entity =>
        {
            entity.ToTable("race_entries");
            entity.HasKey(e => e.id);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.driver_id).IsRequired();
            entity.Property(e => e.race_id).IsRequired();
            entity.HasOne(e => e.driver)
                .WithMany(d => d.race_entries)
                .HasForeignKey(e => e.driver_id)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.race)
                .WithMany(r => r.race_entries)
                .HasForeignKey(e => e.race_id)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.laps)
                .WithOne(l => l.race_entry)
                .HasForeignKey(l => l.race_entry_id)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.pit_stops)
                .WithOne(s => s.race_entry)
                .HasForeignKey(s => s.race_entry_id)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.race_result)
                .WithOne(r => r.race_entry)
                .HasForeignKey<RaceResult>(r => r.race_entry_id)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.driver_id, e.race_id }).IsUnique();
        });

        modelBuilder.Entity<RaceResult>(entity =>
        {
            entity.ToTable("race_results");
            entity.HasKey(e => e.id);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.race_entry_id).IsRequired();
            entity.Property(e => e.start_position).IsRequired();
            entity.Property(e => e.finish_position).IsRequired();
            entity.Property(e => e.has_fastest_lap).HasDefaultValue(false);
            entity.Property(e => e.penalties).IsRequired();
            entity.Property(e => e.dnf).HasDefaultValue(false);
            entity.Property(e => e.points).IsRequired();
            entity.Property(e => e.damage).IsRequired();
            entity.HasOne(e => e.race_entry)
                .WithOne(re => re.race_result)
                .HasForeignKey<RaceResult>(e => e.race_entry_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lap>(entity =>
        {
            entity.ToTable("laps");
            entity.HasKey(e => e.id);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.race_entry_id).IsRequired();
            entity.Property(e => e.lap_number).IsRequired();
            entity.Property(e => e.current_position).IsRequired();
            entity.Property(e => e.delta_leader).IsRequired();
            entity.Property(e => e.delta_front).IsRequired();
            entity.Property(e => e.last_lap_time).IsRequired();
            entity.Property(e => e.tyre_wear).IsRequired();
            entity.Property(e => e.tyre_compound_id).IsRequired();
            entity.HasOne(e => e.race_entry)
                .WithMany(re => re.laps)
                .HasForeignKey(e => e.race_entry_id)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.tyre_compound)
                .WithMany(re => re.laps)
                .HasForeignKey(e => e.tyre_compound_id)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.race_entry_id, e.lap_number }).IsUnique();
        });

        modelBuilder.Entity<PitStop>(entity =>
        {
            entity.ToTable("pit_stops");
            entity.HasKey(e => e.id);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.race_entry_id).IsRequired();
            entity.Property(e => e.lap_number).IsRequired();
            entity.Property(e => e.pit_stop_time).IsRequired();
            entity.HasOne(e => e.race_entry)
                .WithMany(re => re.pit_stops)
                .HasForeignKey(e => e.race_entry_id)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.race_entry_id, e.lap_number }).IsUnique();
        });
    }
}