using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Entities.Models;

public class AppUser : IdentityUser
{
    public long? CreatedBy { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public long? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Library> Libraries { get; private set; } = new List<Library>();
}
