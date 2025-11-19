using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Entities.Models;

public class Library
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string LibraryName { get; set; } = default!;
    public string Location { get; set; } = default!;
    public string ContactNo { get; set; } = default!;

    [Required]
    public string AppUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(AppUserId))]
    public virtual AppUser ApplicationUser { get; set; } = null!;

    public virtual ICollection<Book> Books { get; private set; } = new HashSet<Book>();
}
