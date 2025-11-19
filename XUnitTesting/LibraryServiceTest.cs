using FluentAssertions;
using LibraryManagementSystem.DataDbContext;
using LibraryManagementSystem.Entities.Models;
using LibraryManagementSystem.Repository.Service;
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
