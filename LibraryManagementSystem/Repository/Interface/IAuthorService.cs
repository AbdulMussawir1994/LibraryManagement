using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Helpers;

namespace LibraryManagementSystem.Repository.Interface;

public interface IAuthorService
{
    Task<MobileResponse<IEnumerable<AuthorDto>>> GetAllAuthorsAsync(CancellationToken ct);
    Task<MobileResponse<AuthorDto>> GetAuthorByIdAsync(int id, CancellationToken ct);
    Task<MobileResponse<AuthorDto>> CreateAuthorAsync(CreateAuthorDto dto, CancellationToken ct);
    Task<MobileResponse<AuthorDto>> UpdateAuthorAsync(UpdateAuthorDto dto, CancellationToken ct);
    Task<MobileResponse<bool>> DeleteAuthorAsync(int id, CancellationToken ct);
}
