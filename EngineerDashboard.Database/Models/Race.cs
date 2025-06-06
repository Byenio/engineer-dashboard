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

    public int trackid { get; set; }

    [Required]
    public int aidifficulty { get; set; }

    [Required]
    public int racelength { get; set; }

    [ForeignKey("trackid")]
    public virtual Track track { get; set; } = null!;

    public virtual ICollection<RaceEntry> raceentries { get; set; } = new List<RaceEntry>();
}