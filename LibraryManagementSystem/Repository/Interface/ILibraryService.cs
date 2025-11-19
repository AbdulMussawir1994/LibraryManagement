using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Helpers;

namespace LibraryManagementSystem.Repository.Interface;

public interface ILibraryService
{
    Task<MobileResponse<IEnumerable<LibraryDto>>> GetAllLibrariesAsync(CancellationToken cancellationToken);
    Task<MobileResponse<LibraryDto>> GetLibraryByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<MobileResponse<LibraryDto>> CreateLibraryAsync(CreateLibraryDto dto, CancellationToken cancellationToken);
    Task<MobileResponse<LibraryDto>> UpdateLibraryAsync(UpdateLibraryDto dto, CancellationToken cancellationToken);
    Task<MobileResponse<bool>> DeleteLibraryAsync(Guid id, CancellationToken cancellationToken);
}
