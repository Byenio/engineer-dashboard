using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineerDashboard.Database.Models;

public class Rank
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    [Required]
    [MaxLength(50)]
    public string name { get; set; } = null!;
    [MaxLength(255)]
    public string? icon { get; set; }
    
    [Required]
    [Column("min_points")]
    public int min_points { get; set; }
    
    [Column("max_points")]
    public int? max_points { get; set; }

    public virtual ICollection<Driver> drivers { get; set; } = new List<Driver>();
}
