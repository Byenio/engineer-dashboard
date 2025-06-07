using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineerDashboard.Database.Models;

public class RaceEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    public int driverid { get; set; }

    public int raceid { get; set; }

    public int teamid { get; set; }

    public int startposition { get; set; }

    [ForeignKey("driverid")]
    public virtual Driver driver { get; set; } = null!;

    [ForeignKey("raceid")]
    public virtual Race race { get; set; } = null!;

    [ForeignKey("teamid")]
    public virtual Team? team { get; set; }

    public virtual ICollection<Lap> laps { get; set; } = new List<Lap>();
    public virtual ICollection<Stint> stints { get; set; } = new List<Stint>();
    public virtual RaceResult? raceresult { get; set; }
}