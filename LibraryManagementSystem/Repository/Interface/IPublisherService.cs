using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Helpers;

namespace LibraryManagementSystem.Repository.Interface;

public interface IPublisherService
{
    Task<MobileResponse<IEnumerable<PublisherDto>>> GetAllPublishersAsync(CancellationToken ct);
    Task<MobileResponse<PublisherDto>> GetPublisherByIdAsync(int id, CancellationToken ct);
    Task<MobileResponse<PublisherDto>> CreatePublisherAsync(CreatePublisherDto dto, CancellationToken ct);
    Task<MobileResponse<PublisherDto>> UpdatePublisherAsync(UpdatePublisherDto dto, CancellationToken ct);
    Task<MobileResponse<bool>> DeletePublisherAsync(int id, CancellationToken ct);
}
