using LibraryManagementSystem.DataDbContext;
using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Entities.Models;
using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repository.Service;

public class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _context;

    public AuthorService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<MobileResponse<IEnumerable<AuthorDto>>> GetAllAuthorsAsync(CancellationToken ct)
    {
        var authors = await _context.Authors.AsNoTracking()
            .Select(x => new AuthorDto(x.Id, x.Name, x.Biography, x.DateCreated))
            .ToListAsync(ct);

        return authors.Any()
            ? MobileResponse<IEnumerable<AuthorDto>>.Success(authors, "Fetched successfully.", "SUCCESS-200")
            : MobileResponse<IEnumerable<AuthorDto>>.EmptyValues(Enumerable.Empty<AuthorDto>(), "", "NotFound-404");
    }

    public async Task<MobileResponse<AuthorDto>> GetAuthorByIdAsync(int id, CancellationToken ct)
    {
        var author = await _context.Authors.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AuthorDto(x.Id, x.Name, x.Biography, x.DateCreated))
            .FirstOrDefaultAsync(ct);

        return author is null
            ? MobileResponse<AuthorDto>.Fail("Author not found.", "ERROR-404")
            : MobileResponse<AuthorDto>.Success(author, "Fetched successfully.", "SUCCESS-200");
    }

    public async Task<MobileResponse<AuthorDto>> CreateAuthorAsync(CreateAuthorDto dto, CancellationToken ct)
    {
        var entity = new Author
        {
            Name = dto.Name.Trim(),
            Biography = dto.Biography.Trim(),
            DateCreated = DateTime.UtcNow
        };

        try
        {
            await _context.Authors.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return MobileResponse<AuthorDto>.Success(
                new AuthorDto(entity.Id, entity.Name, entity.Biography, entity.DateCreated),
                "Author created successfully.",
                "SUCCESS-201"
            );
        }
        catch (Exception ex)
        {
            return MobileResponse<AuthorDto>.Fail($"Failed to create author: {ex.Message}", "ERROR-500");
        }
    }

    public async Task<MobileResponse<AuthorDto>> UpdateAuthorAsync(UpdateAuthorDto dto, CancellationToken ct)
    {
        var entity = await _context.Authors.FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

        if (entity is null)
            return MobileResponse<AuthorDto>.Fail("Author not found.", "ERROR-404");

        entity.Name = dto.Name.Trim();
        entity.Biography = dto.Biography.Trim();

        await _context.SaveChangesAsync(ct);

        return MobileResponse<AuthorDto>.Success(
            new AuthorDto(entity.Id, entity.Name, entity.Biography, entity.DateCreated),
            "Author updated successfully.",
            "SUCCESS-200"
        );
    }

    public async Task<MobileResponse<bool>> DeleteAuthorAsync(int id, CancellationToken ct)
    {
        try
        {
            var entity = await _context.Authors.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
                return MobileResponse<bool>.Fail("Author not found.", "ERROR-404");

            _context.Authors.Remove(entity);
            await _context.SaveChangesAsync(ct);

            return MobileResponse<bool>.Success(true, "Author deleted successfully.", "SUCCESS-200");
        }
        catch (Exception ex)
        {
            return MobileResponse<bool>.ExceptionFailed($"Failed to delete author: {ex.Message}", "ERROR-500");
        }
    }
}
