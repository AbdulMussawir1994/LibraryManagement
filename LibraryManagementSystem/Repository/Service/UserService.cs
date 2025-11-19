using LibraryManagementSystem.DataDbContext;
using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Entities.Models;
using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Repository.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryManagementSystem.Repository.Service;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly LibraryDbContext _db;
    private readonly SymmetricSecurityKey _cachedKey;

    public UserService(UserManager<AppUser> userManager, IConfiguration configuration, LibraryDbContext libraryDbContext)
    {
        _userManager = userManager;
        _configuration = configuration;
        _db = libraryDbContext;

        var secret = _configuration["JWTKey:Secret"] ?? throw new InvalidOperationException("JWT secret is missing.");
        _cachedKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    public async Task<MobileResponse<LoginResponseModelDto>> LoginUserAsync(LoginViewModelDto model, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(model.Username).ConfigureAwait(false);

        if (user is null)
            return MobileResponse<LoginResponseModelDto>.Fail("Invalid username.", "Error-404");

        if (await _userManager.IsLockedOutAsync(user).ConfigureAwait(false))
            return MobileResponse<LoginResponseModelDto>.Fail("Account locked. Try again after 60 minutes.", "Error-400");

        if (!await _userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false))
        {
            await _userManager.AccessFailedAsync(user).ConfigureAwait(false);

            if (await _userManager.IsLockedOutAsync(user).ConfigureAwait(false))
                return MobileResponse<LoginResponseModelDto>.Fail("Account locked. Try again after 60 minutes.", "Error-400");

            return MobileResponse<LoginResponseModelDto>.Fail("Invalid password.", "Error-404");
        }

        await _userManager.ResetAccessFailedCountAsync(user).ConfigureAwait(false);

        var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        if (roles is null || roles.Count == 0)
            roles = new List<string> { "User" };

        var token = GenerateKmacJwtToken(user.Id, roles, user.Email);

        return MobileResponse<LoginResponseModelDto>.Success(
            new LoginResponseModelDto { token = token },
            "Login successful.",
            "SUCCESS-200"
        );
    }

    public async Task<MobileResponse<string>> RegisterUserAsync(RegisterViewModelDto model, CancellationToken cancellationToken)
    {

        var checkEmail = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);

        if (checkEmail is not null)
            return MobileResponse<string>.Fail("Email is already registered.", "Error-400");

        var checkUsername = await _userManager.FindByNameAsync(model.Email).ConfigureAwait(false);

        if (checkUsername is not null)
            return MobileResponse<string>.Fail("Username is already registered.", "Error-400");


        var user = new AppUser
        {
            UserName = model.Username.Trim(),
            Email = model.Email.Trim(),
            DateCreated = DateTime.UtcNow,
            LockoutEnabled = true
        };

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var result = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return MobileResponse<string>.Fail(result.Errors.FirstOrDefault()?.Description ?? "Unknown error.", "Error-400");
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return MobileResponse<string>.Success(model.Username, "Register successful.", "SUCCESS-200");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return MobileResponse<string>.ExceptionFailed(ex.Message, "Error-500");
        }
    }

    public string GenerateKmacJwtToken(string userId, IEnumerable<string> roles, string email)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));

        var utcNow = DateTime.UtcNow;
        var issuer = _configuration["JWTKey:ValidIssuer"];
        var audience = _configuration["JWTKey:ValidAudience"];
        var expiryMinutes = int.Parse(_configuration["JWTKey:TokenExpiryTimeInMinutes"] ?? "30");
        var secret = _configuration["JWTKey:Secret"] ?? throw new InvalidOperationException("JWT secret is missing.");
        var baseKey = Encoding.UTF8.GetBytes(secret);

        // Ensure deterministic ordering
        var roleList = (roles ?? Enumerable.Empty<string>()).OrderBy(r => r).ToList();

        // ✅ Always derive the signing key based on userId|roles|email
        var derivedKey = KmacSecurity.DeriveKmacKey(userId, roleList, email, baseKey);

        var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, userId),
        new(JwtRegisteredClaimNames.Email, email),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        new(ClaimTypes.NameIdentifier, userId),
        new(ClaimTypes.Email, email)
    };

        claims.AddRange(roleList.Select(role => new Claim("role", role)));

        var credentials = new SigningCredentials(new SymmetricSecurityKey(derivedKey), SecurityAlgorithms.HmacSha512);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = utcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}