using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineerDashboard.Database.Models;

public class Driver
{
    [Key]
    public int id { get; set; }

    [Required]
    [MaxLength(100)]
    public string name { get; set; } = null!;

    [Required]
    public int? elo { get; set; } = 1000;

    public int rankid { get; set; } = 2;

    public int? teamid { get; set; }

    [ForeignKey("rankid")]
    public virtual Rank rank { get; set; } = null!;

    [ForeignKey("teamid")]
    public virtual Team? team { get; set; }

    public virtual ICollection<RaceEntry> raceentries { get; set; } = new List<RaceEntry>();
}