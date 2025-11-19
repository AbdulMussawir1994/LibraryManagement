using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Entities.Models;

public class Author
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public string Biography { get; set; } = string.Empty;

    public virtual ICollection<Book> Books { get; private set; } = new HashSet<Book>();
}