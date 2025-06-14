using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineerDashboard.Database.Models;

public class Race
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    [Required]
    public DateTime date { get; set; }
    
    [Column("track_id")]
    public int track_id { get; set; }
    
    [Required]
    [Column("ai_difficulty")]
    public int ai_difficulty { get; set; }
    [Required]
    public int length { get; set; }

    [ForeignKey("track_id")]
    public virtual Track track { get; set; } = null!;

    public virtual ICollection<RaceEntry> race_entries { get; set; } = new List<RaceEntry>();
}