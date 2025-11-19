using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Helpers;

namespace LibraryManagementSystem.Repository.Interface;

public interface IUserService
{
    Task<MobileResponse<LoginResponseModelDto>> LoginUserAsync(LoginViewModelDto model, CancellationToken cancellationToken);
    Task<MobileResponse<string>> RegisterUserAsync(RegisterViewModelDto model, CancellationToken cancellationToken);
}
