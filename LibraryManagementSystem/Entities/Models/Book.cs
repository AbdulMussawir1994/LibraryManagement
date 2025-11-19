using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Entities.Models;

public class Book
{
    [Key]
    public int Id { get; set; }

    public string BookName { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Language { get; set; } = default!;
    public int AvailableBooks { get; set; }

    public Guid LibraryId { get; set; }
    public int AuthorId { get; set; }
    public int PublisherId { get; set; }

    public virtual Library Library { get; set; } = null!;
    public virtual Author Author { get; set; } = null!;
    public virtual Publisher Publisher { get; set; } = null!;
}