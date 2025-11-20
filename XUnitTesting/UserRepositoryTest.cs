using LibraryManagementSystem.DataDbContext;
using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Entities.Models;
using LibraryManagementSystem.Repository.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace XUnitTesting;

public class UserRepositoryTest
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly IConfiguration _config;
    private readonly LibraryDbContext _db;

    public UserRepositoryTest()
    {
        // Mock UserManager
        var store = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        // Mock IConfiguration
        var inMemorySettings = new Dictionary<string, string>
        {
            {"JWTKey:Secret", "ThisIsAStrongSecretKey12345"},
            {"JWTKey:ValidIssuer", "TestIssuer"},
            {"JWTKey:ValidAudience", "TestAudience"},
            {"JWTKey:TokenExpiryTimeInMinutes", "30"}
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // InMemory DB
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _db = new LibraryDbContext(options);
    }

    private UserService CreateService() =>
        new UserService(_userManagerMock.Object, _config, _db);

    [Fact]
    public async Task Login_ShouldFail_WhenUsernameNotFound()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser)null);

        var service = CreateService();

        // Act
        var response = await service.LoginUserAsync(
            new LoginViewModelDto { Username = "abc", Password = "pass" },
            CancellationToken.None);

        // Assert
        Assert.False(response.Status);
        Assert.Equal("Invalid username.", response.Message);
    }

    [Fact]
    public async Task Login_ShouldFail_WhenInvalidPassword()
    {
        var user = new AppUser { Id = "1", UserName = "test" };

        _userManagerMock.Setup(x => x.FindByNameAsync("test"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "wrong"))
            .ReturnsAsync(false);

        var service = CreateService();

        var response = await service.LoginUserAsync(
            new LoginViewModelDto { Username = "test", Password = "wrong" },
            CancellationToken.None);

        Assert.False(response.Status);
        Assert.Equal("Invalid password.", response.Message);
    }

    [Fact]
    public async Task Login_ShouldFail_WhenUserLockedOut()
    {
        var user = new AppUser { Id = "1", UserName = "test" };

        _userManagerMock.Setup(x => x.FindByNameAsync("test"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(true);

        var service = CreateService();

        var response = await service.LoginUserAsync(
            new LoginViewModelDto { Username = "test", Password = "123" },
            CancellationToken.None);

        Assert.False(response.Status);
        Assert.Equal("Account locked. Try again after 60 minutes.", response.Message);
    }

    [Fact]
    public async Task Login_ShouldSuccess_WhenCredentialsAreCorrect()
    {
        var user = new AppUser { Id = "1", UserName = "test", Email = "test@mail.com" };

        _userManagerMock.Setup(x => x.FindByNameAsync("test"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(false);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "123"))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        var service = CreateService();

        var response = await service.LoginUserAsync(
            new LoginViewModelDto { Username = "test", Password = "123" },
            CancellationToken.None);

        Assert.True(response.Status);
        Assert.NotNull(response.Data.token);
        Assert.Contains("ey", response.Data.token); // Basic JWT structure check
    }

    [Fact]
    public async Task Register_ShouldFail_WhenEmailExists()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@mail.com"))
            .ReturnsAsync(new AppUser());

        var service = CreateService();

        var response = await service.RegisterUserAsync(
            new RegisterViewModelDto
            {
                Username = "test",
                Email = "test@mail.com",
                Password = "P@ss123"
            }, CancellationToken.None);

        Assert.False(response.Status);
        Assert.Equal("Email is already registered.", response.Message);
    }

    [Fact]
    public async Task Register_ShouldFail_WhenUsernameExists()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser)null);

        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new AppUser());

        var service = CreateService();

        var response = await service.RegisterUserAsync(
            new RegisterViewModelDto
            {
                Username = "test",
                Email = "user@mail.com",
                Password = "P@ss123"
            }, CancellationToken.None);

        Assert.False(response.Status);
        Assert.Equal("Username is already registered.", response.Message);
    }
}

public class UserServiceXTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly IConfiguration _config;
    private readonly LibraryDbContext _db;

    public UserServiceXTests()
    {
        var store = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        var settings = new Dictionary<string, string>
        {
            {"JWTKey:Secret", "ThisIsAStrongSecretKey12345"},
            {"JWTKey:ValidIssuer", "TestIssuer"},
            {"JWTKey:ValidAudience", "TestAudience"},
            {"JWTKey:TokenExpiryTimeInMinutes", "30"}
        };

        _config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(connection)
            .Options;

        _db = new LibraryDbContext(options);
        _db.Database.EnsureCreated();
    }

    private UserService CreateService() =>
        new UserService(_userManagerMock.Object, _config, _db);

    [Fact]
    public async Task Register_ShouldSuccess_WhenValid()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser)null);

        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        var response = await service.RegisterUserAsync(
            new RegisterViewModelDto
            {
                Username = "testuser",
                Email = "user@mail.com",
                Password = "P@ss123"
            }, CancellationToken.None);

        Assert.True(response.Status);
        Assert.Equal("Register successful.", response.Message);
    }
}