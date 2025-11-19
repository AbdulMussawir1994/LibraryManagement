namespace LibraryManagementSystem.Entities.DTOs;

public readonly record struct LoginViewModelDto
{
    public string Username { get; init; }
    public string Password { get; init; }
}

public readonly record struct LoginResponseModelDto(string token);
