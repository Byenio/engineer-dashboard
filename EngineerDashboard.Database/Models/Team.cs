using System.ComponentModel.DataAnnotations;

namespace EngineerDashboard.Database.Models;

public class Team
{
    [Key]
    public int id { get; set; }

    [Required]
    [MaxLength(100)]
    public string name { get; set; } = null!;

    public virtual ICollection<Driver> drivers { get; set; } = new List<Driver>();
    public virtual ICollection<RaceEntry> raceentries { get; set; } = new List<RaceEntry>();
}