using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineerDashboard.Database.Models;

public class RaceEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    
    [Column("driver_id")]
    public int driver_id { get; set; }
    
    [Column("race_id")]
    public int race_id { get; set; }
    
    [ForeignKey("driver_id")]
    public virtual Driver driver { get; set; } = null!;
    
    [ForeignKey("race_id")]
    public virtual Race race { get; set; } = null!;

    public virtual ICollection<Lap> laps { get; set; } = new List<Lap>();
    public virtual ICollection<PitStop> pit_stops { get; set; } = new List<PitStop>();
    public virtual RaceResult? race_result { get; set; }
}