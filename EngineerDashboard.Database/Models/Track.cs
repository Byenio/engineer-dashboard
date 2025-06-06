using System.ComponentModel.DataAnnotations;

namespace EngineerDashboard.Database.Models;

public class Track
{
    [Key]
    public int id { get; set; }

    [Required]
    [MaxLength(100)]
    public string name { get; set; } = null!;

    public virtual ICollection<Race> races { get; set; } = new List<Race>();
}