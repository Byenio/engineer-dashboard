using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineerDashboard.Database.Models;

public class Lap
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    public int raceentryid { get; set; }

    public int lapnum { get; set; }

    public int currentposition { get; set; }

    public int? deltatoleader { get; set; }

    public int? deltatocarinfront { get; set; }

    public int? lastlaptime { get; set; }

    public int? tyrewear { get; set; }

    [ForeignKey("raceentryid")]
    public virtual RaceEntry raceentry { get; set; } = null!;
}