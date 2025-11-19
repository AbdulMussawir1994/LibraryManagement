using LibraryManagementSystem.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibraryManagementSystem.DataDbContext;

public class LibraryDbContext : IdentityDbContext<AppUser>
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
        try
        {
            var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;

            if (databaseCreator is not null)
            {
                if (!databaseCreator.CanConnect()) databaseCreator.Create();
                if (!databaseCreator.HasTables()) databaseCreator.CreateTables();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique().HasDatabaseName("IDX_Email");
            entity.HasIndex(u => u.UserName).IsUnique().HasDatabaseName("IDX_UserName");

            entity.Property(e => e.DateCreated)
                  .HasDefaultValueSql("GETUTCDATE()") // Ensures DB defaulting
                  .IsRequired();

            entity.Property(e => e.UpdatedDate)
                  .HasDefaultValueSql("NULL"); // Ensures first-time NULL

            entity.Property(e => e.LockoutEnabled)
            .HasDefaultValue(true);
        });

        modelBuilder.Entity<Library>(entity =>
        {
            entity.Property(l => l.LibraryName)
                  .HasMaxLength(30)
                  .IsRequired();

            entity.Property(l => l.Location)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(l => l.ContactNo)
                  .HasMaxLength(20)
                  .IsRequired();

            //entity.HasOne(l => l.AppUsers)
            //      .WithMany()
            //      .HasForeignKey(l => l.UserId)
            //      .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(l => l.LibraryName);
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(a => a.Name)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(a => a.Biography)
                  .HasMaxLength(50);

            entity.HasIndex(a => a.Name);
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.Property(p => p.Name)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.Property(p => p.Address)
                  .HasMaxLength(100);

            entity.Property(p => p.ContactInfo)
                  .HasMaxLength(20);

            entity.HasIndex(p => p.Name);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(b => b.BookName)
                  .HasMaxLength(150)
                  .IsRequired();

            entity.Property(b => b.Title)
                  .HasMaxLength(150)
                  .IsRequired();

            entity.Property(b => b.Language)
                  .HasMaxLength(50);

            entity.HasIndex(b => b.BookName);
            entity.HasIndex(b => b.AuthorId);
            entity.HasIndex(b => b.LibraryId);

            entity.HasOne(b => b.Library)
                  .WithMany(l => l.Books)
                  .HasForeignKey(b => b.LibraryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Author)
                  .WithMany(a => a.Books)
                  .HasForeignKey(b => b.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Publisher)
                  .WithMany(p => p.Books)
                  .HasForeignKey(b => b.PublisherId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Add Auto Migration Seeds
        var defaultUserId = Guid.NewGuid().ToString();
        var defaultLibraryId = Guid.NewGuid();
        var defaultAuthorId = 1;
        var defaultPublisherId = 1;
        var defaultBookId = 1;

        var passwordHasher = new PasswordHasher<AppUser>();
        var seedUser = new AppUser
        {
            Id = defaultUserId,
            UserName = "Admin123",
            NormalizedUserName = "ADMIN123",
            Email = "admin@library.com",
            NormalizedEmail = "ADMIN@LIBRARY.COM",
            EmailConfirmed = true,
            DateCreated = DateTime.UtcNow,
            LockoutEnabled = true
        };
        seedUser.PasswordHash = passwordHasher.HashPassword(seedUser, "Admin@123");
        modelBuilder.Entity<AppUser>().HasData(seedUser);

        modelBuilder.Entity<Library>().HasData(new Library
        {
            Id = defaultLibraryId,
            LibraryName = "Central Library",
            Location = "Main Street",
            ContactNo = "123456789",
            AppUserId = defaultUserId
        });

        modelBuilder.Entity<Author>().HasData(new Author
        {
            Id = defaultAuthorId,
            Name = "Default Author",
            DateCreated = DateTime.UtcNow,
            Biography = "Sample biography for seed author."
        });


        modelBuilder.Entity<Publisher>().HasData(new Publisher
        {
            Id = defaultPublisherId,
            Name = "Default Publisher",
            Address = "Publisher Address",
            ContactInfo = "987654321",
            PublishYear = DateTime.UtcNow.Year
        });

        modelBuilder.Entity<Book>().HasData(new Book
        {
            Id = defaultBookId,
            BookName = "Sample Book",
            Title = "Sample Book Title",
            Language = "English",
            AvailableBooks = 10,
            LibraryId = defaultLibraryId,
            AuthorId = defaultAuthorId,
            PublisherId = defaultPublisherId
        });
    }

    public DbSet<Library> Libraries => Set<Library>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Publisher> Publishers => Set<Publisher>();

}
