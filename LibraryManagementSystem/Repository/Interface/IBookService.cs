using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Helpers;

namespace LibraryManagementSystem.Repository.Interface;

public interface IBookService
{
    Task<MobileResponse<IEnumerable<BookDto>>> GetAllBooksAsync(CancellationToken ct);
    Task<MobileResponse<BookDto>> GetBookByIdAsync(int id, CancellationToken ct);
    Task<MobileResponse<BookDto>> CreateBookAsync(CreateBookDto dto, CancellationToken ct);
    Task<MobileResponse<BookDto>> UpdateBookAsync(UpdateBookDto dto, CancellationToken ct);
    Task<MobileResponse<bool>> DeleteBookAsync(int id, CancellationToken ct);
}
