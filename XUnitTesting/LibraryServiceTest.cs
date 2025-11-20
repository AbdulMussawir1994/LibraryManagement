using FluentAssertions;
using LibraryManagementSystem.DataDbContext;
using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Entities.Models;
using LibraryManagementSystem.Repository.Service;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace XUnitTesting;

public class LibraryServiceTest
{
    private LibraryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LibraryDbContext(options);
    }

    private LibraryService CreateService(LibraryDbContext db)
        => new LibraryService(db);


    [Fact(DisplayName = "GetAllLibraries_Should_ReturnEmpty_WhenNoData")]
    public async Task GetAllLibraries_Should_ReturnEmpty_WhenNoData()
    {
        // Arrange
        var db = CreateDbContext();
        var service = CreateService(db);

        // Act
        var response = await service.GetAllLibrariesAsync(CancellationToken.None);

        // Assert
        response.Status.Should().BeTrue();
        response.Data.Should().BeNull();
    }

    [Fact(DisplayName = "GetAllLibraries_Should_ReturnData_WhenLibrariesExist")]
    public async Task GetAllLibraries_Should_ReturnData_WhenLibrariesExist()
    {
        // Arrange
        var db = CreateDbContext();
        var service = CreateService(db);

        db.Libraries.Add(new Library
        {
            Id = Guid.NewGuid(),
            LibraryName = "Central Library",
            Location = "Downtown",
            ContactNo = "1234567890",
            AppUserId = "user-123"
        });

        await db.SaveChangesAsync();

        // Act
        var response = await service.GetAllLibrariesAsync(CancellationToken.None);

        // Assert
        response.Status.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(1);

        var library = response.Data.First();
        library.LibraryName.Should().Be("Central Library");
        library.Location.Should().Be("Downtown");
    }
}

public class LibraryServiceTestForCreate
{
    private LibraryDbContext CreateDbContext()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        // Enable foreign key constraints in SQLite
        var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new LibraryDbContext(options);
        db.Database.EnsureCreated();   // Creates tables & relationships

        return db;
    }

    private LibraryService CreateService(LibraryDbContext db)
        => new LibraryService(db);


    [Fact(DisplayName = "New_Library_Creation_Be_Failed_WhenUserIdIsInvalid")]
    public async Task New_Library_Creation_Successful()
    {
        // Arrange
        using var db = CreateDbContext();
        var service = CreateService(db);

        db.Users.Add(new AppUser { Id = "user-123", UserName = "TestUser" });
        await db.SaveChangesAsync();

        var dto = new CreateLibraryDto
        {
            LibraryName = "City Library",
            Location = "Uptown",
            ContactNo = "0987654321",
            UserId = "user-123"
        };

        // Act
        var response = await service.CreateLibraryAsync(dto, CancellationToken.None);

        // Assert
        response.Status.Should().BeTrue();
        response.Message.Should().Be("Library created successfully.");
    }
}