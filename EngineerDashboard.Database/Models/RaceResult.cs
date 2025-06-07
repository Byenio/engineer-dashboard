using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineerDashboard.Database.Models;

public class RaceResult
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public int raceentryid { get; set; }
    
    public int finishposition { get; set; }

    public bool hasfastestlap { get; set; }

    public int penaltiesinseconds { get; set; }

    public bool hasdnf { get; set; }

    public int points { get; set; }

    public int averagedamage { get; set; }
    
    [ForeignKey("raceentryid")]
    public virtual RaceEntry raceentry { get; set; } = null!;
}