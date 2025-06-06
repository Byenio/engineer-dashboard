using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineerDashboard.Database.Models;

public class Stint
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    public int raceentryid { get; set; }

    public int endlap { get; set; }

    public int? tyrecompound { get; set; }

    public int? tyrewear { get; set; }

    public int? pitstoptime { get; set; }
    
    [ForeignKey("raceentryid")]
    public virtual RaceEntry raceentry { get; set; } = null!;
}