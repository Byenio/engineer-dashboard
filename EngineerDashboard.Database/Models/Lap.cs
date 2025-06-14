using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineerDashboard.Database.Models;

public class Lap
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    
    [Column("race_entry_id")]
    public int race_entry_id { get; set; }
    
    [Column("lap_number")]
    public int lap_number { get; set; }
    
    [Column("tyre_wear")]
    public int? tyre_wear { get; set; }
    
    [Column("tyre_compound_id")]
    public int tyre_compound_id { get; set; }
    
    [Column("current_position")]
    public int current_position { get; set; }
    
    [Column("delta_leader")]
    public int? delta_leader { get; set; }
    
    [Column("delta_front")]
    public int? delta_front { get; set; }
    
    [Column("last_lap_time")]
    public int? last_lap_time { get; set; }

    [ForeignKey("race_entry_id")]
    public virtual RaceEntry race_entry { get; set; } = null!;
    
    [ForeignKey("tyre_compound_id")]
    public virtual TyreCompound tyre_compound { get; set; } = null!;
}