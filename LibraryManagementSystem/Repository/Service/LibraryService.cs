using LibraryManagementSystem.DataDbContext;
using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Entities.Models;
using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repository.Service;

public class LibraryService : ILibraryService
{
    private readonly LibraryDbContext _db;

    public LibraryService(LibraryDbContext db)
    {
        _db = db;
    }

    public async Task<MobileResponse<IEnumerable<LibraryDto>>> GetAllLibrariesAsync(CancellationToken cancellationToken)
    {
        var result = await _db.Libraries.AsNoTracking()
            .Select(x => new LibraryDto(x.Id, x.LibraryName, x.Location, x.ContactNo, x.AppUserId)).ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!result.Any())
        {
            return MobileResponse<IEnumerable<LibraryDto>>.EmptyValues(Enumerable.Empty<LibraryDto>());
        }

        return MobileResponse<IEnumerable<LibraryDto>>.Success(result, "Fetched successfully.", "SUCCESS-200");
    }

    public async Task<MobileResponse<LibraryDto>> GetLibraryByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var library = await _db.Libraries
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new LibraryDto(x.Id, x.LibraryName, x.Location, x.ContactNo, x.AppUserId))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (library is null)
            return MobileResponse<LibraryDto>.Fail("Library not found.", "ERROR-404");

        return MobileResponse<LibraryDto>.Success(library, "Fetched successfully.", "SUCCESS-200");
    }

    public async Task<MobileResponse<LibraryDto>> CreateLibraryAsync(CreateLibraryDto dto, CancellationToken cancellationToken)
    {
        var entity = new Library
        {
            LibraryName = dto.LibraryName.Trim(),
            Location = dto.Location.Trim(),
            ContactNo = dto.ContactNo.Trim(),
            AppUserId = dto.UserId,
        };

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _db.Libraries.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return MobileResponse<LibraryDto>.Success(new LibraryDto(entity.Id, entity.LibraryName, entity.Location, entity.ContactNo, entity.AppUserId), "Library created successfully.", "SUCCESS-201");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return MobileResponse<LibraryDto>.ExceptionFailed($"Failed to create library: {ex.Message}", "ERROR-500");
        }
    }

    public async Task<MobileResponse<LibraryDto>> UpdateLibraryAsync(UpdateLibraryDto dto, CancellationToken cancellationToken)
    {
        var entity = await _db.Libraries.FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken).ConfigureAwait(false);

        if (entity is null)
            return MobileResponse<LibraryDto>.Fail("Library not found.", "ERROR-404");

        entity.LibraryName = dto.LibraryName.Trim();
        entity.Location = dto.Location.Trim();
        entity.ContactNo = dto.ContactNo.Trim();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MobileResponse<LibraryDto>.Success(new LibraryDto(entity.Id, entity.LibraryName, entity.Location, entity.ContactNo, entity.AppUserId), "Library updated successfully.", "SUCCESS-200");
    }

    public async Task<MobileResponse<bool>> DeleteLibraryAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _db.Libraries.FirstOrDefaultAsync(l => l.Id == id, cancellationToken).ConfigureAwait(false);
            if (entity is null)
                return MobileResponse<bool>.Fail("Library not found.", "ERROR-404");

            _db.Libraries.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return MobileResponse<bool>.Success(true, "Library deleted successfully.", "SUCCESS-200");
        }
        catch (Exception ex)
        {
            return MobileResponse<bool>.ExceptionFailed($"Failed to delete libraray: {ex.Message}", "ERROR-500");
        }

    }
}
