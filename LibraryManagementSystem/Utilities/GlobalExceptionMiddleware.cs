using LibraryManagementSystem.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace LibraryManagementSystem.Utilities;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IConfiguration _config;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IConfiguration config)
    {
        _next = next;
        _logger = logger;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            Console.WriteLine("GlobalExceptionMiddleware Invoked");
            if (!IsRequestAnonymous(context))
            {
                if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    await WriteErrorResponse(context, HttpStatusCode.Unauthorized, "Unauthorized Request.");
                    return;
                }

                var headerValue = authHeader.ToString();

                if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    await WriteErrorResponse(context, HttpStatusCode.Unauthorized, "Bearer header is missing");
                    return;
                }

                var token = headerValue["Bearer ".Length..].Trim();

                if (string.IsNullOrWhiteSpace(token))
                {
                    await WriteErrorResponse(context, HttpStatusCode.Unauthorized, "Token not provided.");
                    return;
                }

                var (principal, error) = ValidateTokenAndGetPrincipal(token);

                if (principal is null)
                {
                    await WriteErrorResponse(context, HttpStatusCode.Unauthorized, error ?? "Unauthorized Error");
                    return;
                }

                context.User = principal;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static bool IsRequestAnonymous(HttpContext context)
    {
        return context.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() != null;
    }

    //for KMAC
    public (ClaimsPrincipal? Principal, string? ErrorMessage) ValidateTokenAndGetPrincipal(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return (null, "JWT token is missing.");

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = _config["JWTKey:Secret"] ?? throw new InvalidOperationException("JWT secret is missing.");
            var issuer = _config["JWTKey:ValidIssuer"];
            var audience = _config["JWTKey:ValidAudience"];
            var baseKey = Encoding.UTF8.GetBytes(secret);

            var jwtToken = tokenHandler.ReadJwtToken(jwt);

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
                         ?? jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value
                        ?? jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var roles = jwtToken.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type.Equals("role", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .OrderBy(r => r) // ✅ must match generation order
                .ToList();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(email) || roles.Count == 0)
                return (null, "Token is missing required claims.");

            var derivedKey = KmacSecurity.DeriveKmacKey(userId, roles, email, baseKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(derivedKey),

                ValidateIssuer = true,
                ValidIssuer = issuer,

                ValidateAudience = true,
                ValidAudience = audience,

                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            var principal = tokenHandler.ValidateToken(jwt, validationParameters, out _);
            return (principal, null);
        }
        catch (SecurityTokenExpiredException)
        {
            return (null, "JWT token has expired.");
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return (null, "JWT token signature is invalid.");
        }
        catch (SecurityTokenValidationException ex)
        {
            return (null, $"JWT validation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (null, $"Unexpected error during token validation: {ex.Message}");
        }
    }

    private static Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        var response = new { StatusCode = (int)statusCode, Message = message };
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            NotFoundException => new { StatusCode = (int)HttpStatusCode.NotFound, Message = exception.Message },
            ValidationException => new { StatusCode = (int)HttpStatusCode.BadRequest, Message = exception.Message },
            _ => new { StatusCode = (int)HttpStatusCode.InternalServerError, Message = "An unexpected error occurred. Please try again later." }
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    // ✅ Custom exceptions remain same
    public class NotFoundException : Exception { public NotFoundException(string message) : base(message) { } }
    public class ValidationException : Exception { public ValidationException(string message) : base(message) { } }

}
