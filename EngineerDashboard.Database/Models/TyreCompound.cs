using System.ComponentModel.DataAnnotations;

namespace EngineerDashboard.Database.Models;

public class TyreCompound
{
    [Key]
    public int id { get; set; }

    [Required]
    [MaxLength(10)]
    public string name { get; set; } = null!;
    
    public virtual ICollection<Lap> laps { get; set; } = new List<Lap>();
}