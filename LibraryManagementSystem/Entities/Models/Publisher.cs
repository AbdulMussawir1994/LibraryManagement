using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Entities.Models;

public class Publisher
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Address { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;

    public int PublishYear { get; set; } = DateTime.UtcNow.Year;

    public virtual ICollection<Book> Books { get; private set; } = new HashSet<Book>();
}