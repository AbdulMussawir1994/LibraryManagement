using FluentValidation;
using LibraryManagementSystem.Entities.DTOs;

namespace LibraryManagementSystem.Utilities;

public class ModelValidator { }


public class RegisterModelValidator : AbstractValidator<RegisterViewModelDto>
{
    public RegisterModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        //   .WithMessage("Email is already registered.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(6).WithMessage("Username must be at least 6 characters.")
            .MaximumLength(30).WithMessage("Username must not exceed 30 characters.");


        RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Password is required.")
                    .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
                    .MaximumLength(15).WithMessage("Password cannot be more than 15 characters long.")
                    .Matches(@"^[A-Z]").WithMessage("Password must start with an uppercase letter.")
                    .Matches(@"[!@#$%^&*(),.?""{}|<>]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm Password is required.")
            .Equal(x => x.Password).WithMessage("Password and Confirm Password do not match.");
    }
}

public class LoginViewModelValidator : AbstractValidator<LoginViewModelDto>
{
    public LoginViewModelValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(6).WithMessage("Username must be at least 6 characters.")
            .MaximumLength(30).WithMessage("Username must not exceed 30 characters.");

        RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Password is required.");

    }
}

public class CreateLibraryDtoValidator : AbstractValidator<CreateLibraryDto>
{
    public CreateLibraryDtoValidator()
    {
        RuleFor(x => x.LibraryName)
            .NotEmpty().WithMessage("Library name is required.")
            .MaximumLength(30);

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(100);

        RuleFor(x => x.ContactNo)
            .NotEmpty().WithMessage("Contact number is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}

public class CreateAuthorDtoValidator : AbstractValidator<CreateAuthorDto>
{
    public CreateAuthorDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Author name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Biography)
            .MaximumLength(50).WithMessage("Biography must be under 50 characters.");
    }
}

public class CreatePublisherDtoValidator : AbstractValidator<CreatePublisherDto>
{
    public CreatePublisherDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Publisher name is required.")
            .MaximumLength(50);

        RuleFor(x => x.Address)
            .MaximumLength(100);

        RuleFor(x => x.ContactNo)
            .NotEmpty().WithMessage("Contact number is required.");

        RuleFor(x => x.PublishYear)
            .InclusiveBetween(1500, DateTime.UtcNow.Year)
            .WithMessage("Publish year must be valid.");
    }
}

public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookDtoValidator()
    {
        RuleFor(x => x.BookName)
            .NotEmpty().WithMessage("Book name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(150);

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .MaximumLength(50);

        RuleFor(x => x.AvailableBooks)
            .GreaterThanOrEqualTo(0).WithMessage("Available books must be a positive number.");

        RuleFor(x => x.LibraryId)
            .NotEmpty().WithMessage("LibraryId is required.");

        RuleFor(x => x.AuthorId)
            .GreaterThan(0).WithMessage("AuthorId is required.");

        RuleFor(x => x.PublisherId)
            .GreaterThan(0).WithMessage("PublisherId is required.");
    }
}
