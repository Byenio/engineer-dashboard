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
    
    [Column("rank_id")]
    public int rank_id { get; set; } = 2;
    
    [Column("team_id")]
    public int? team_id { get; set; }
    
    [ForeignKey("rank_id")]
    public virtual Rank rank { get; set; } = null!;

    [ForeignKey("team_id")]
    public virtual Team? team { get; set; }

    public virtual ICollection<RaceEntry> race_entries { get; set; } = new List<RaceEntry>();
}