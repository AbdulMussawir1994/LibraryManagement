using LibraryManagementSystem.DataDbContext;
using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Entities.Models;
using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repository.Service;


public class PublisherService : IPublisherService
{
    private readonly LibraryDbContext _context;

    public PublisherService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<MobileResponse<IEnumerable<PublisherDto>>> GetAllPublishersAsync(CancellationToken ct)
    {
        var publishers = await _context.Publishers.AsNoTracking()
            .Select(x => new PublisherDto(x.Id, x.Name, x.Address, x.ContactInfo, x.PublishYear))
            .ToListAsync(ct);

        return publishers.Any()
            ? MobileResponse<IEnumerable<PublisherDto>>.Success(publishers, "Fetched successfully.", "SUCCESS-200")
            : MobileResponse<IEnumerable<PublisherDto>>.EmptyValues(Enumerable.Empty<PublisherDto>());
    }

    public async Task<MobileResponse<PublisherDto>> GetPublisherByIdAsync(int id, CancellationToken ct)
    {
        var publisher = await _context.Publishers.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PublisherDto(x.Id, x.Name, x.Address, x.ContactInfo, x.PublishYear))
            .FirstOrDefaultAsync(ct);

        return publisher is null
            ? MobileResponse<PublisherDto>.Fail("Publisher not found.", "ERROR-404")
            : MobileResponse<PublisherDto>.Success(publisher, "Fetched successfully.", "SUCCESS-200");
    }

    public async Task<MobileResponse<PublisherDto>> CreatePublisherAsync(CreatePublisherDto dto, CancellationToken ct)
    {
        var entity = new Publisher
        {
            Name = dto.Name.Trim(),
            Address = dto.Address.Trim(),
            ContactInfo = dto.ContactNo.Trim(),
            PublishYear = dto.PublishYear
        };

        try
        {
            await _context.Publishers.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return MobileResponse<PublisherDto>.Success(
                new PublisherDto(entity.Id, entity.Name, entity.Address, entity.ContactInfo, entity.PublishYear),
                "Publisher created successfully.",
                "SUCCESS-201"
            );
        }
        catch (Exception ex)
        {
            return MobileResponse<PublisherDto>.Fail($"Failed to create publisher: {ex.Message}", "ERROR-500");
        }
    }

    public async Task<MobileResponse<PublisherDto>> UpdatePublisherAsync(UpdatePublisherDto dto, CancellationToken ct)
    {
        var entity = await _context.Publishers.FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

        if (entity is null)
            return MobileResponse<PublisherDto>.Fail("Publisher not found.", "ERROR-404");

        entity.Name = dto.Name.Trim();
        entity.Address = dto.Address.Trim();
        entity.ContactInfo = dto.ContactInfo.Trim();
        entity.PublishYear = dto.PublishYear;

        await _context.SaveChangesAsync(ct);

        return MobileResponse<PublisherDto>.Success(
            new PublisherDto(entity.Id, entity.Name, entity.Address, entity.ContactInfo, entity.PublishYear),
            "Publisher updated successfully.",
            "SUCCESS-200"
        );
    }

    public async Task<MobileResponse<bool>> DeletePublisherAsync(int id, CancellationToken ct)
    {
        try
        {
            var entity = await _context.Publishers.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
                return MobileResponse<bool>.Fail("Publisher not found.", "ERROR-404");

            _context.Publishers.Remove(entity);
            await _context.SaveChangesAsync(ct);

            return MobileResponse<bool>.Success(true, "Publisher deleted successfully.", "SUCCESS-200");
        }
        catch (Exception ex)
        {
            return MobileResponse<bool>.ExceptionFailed($"Failed to delete publisher: {ex.Message}", "ERROR-500");
        }

    }
}
