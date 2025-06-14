using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineerDashboard.Database.Models;

public class PitStop
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    
    [Column("race_entry_id")]
    public int race_entry_id { get; set; }
    
    [Column("lap_number")]
    public int lap_number { get; set; }
    
    [Column("pit_stop_time")]
    public int? pit_stop_time { get; set; }
    
    [ForeignKey("race_entry_id")]
    public virtual RaceEntry race_entry { get; set; } = null!;
}