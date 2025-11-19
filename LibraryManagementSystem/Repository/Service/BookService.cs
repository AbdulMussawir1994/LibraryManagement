using LibraryManagementSystem.DataDbContext;
using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Entities.Models;
using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repository.Service;

public class BookService : IBookService
{
    private readonly LibraryDbContext _context;

    public BookService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<MobileResponse<IEnumerable<BookDto>>> GetAllBooksAsync(CancellationToken ct)
    {
        var books = await _context.Books.AsNoTracking()
            .Select(x => new BookDto(
                x.Id,
                x.BookName,
                x.Title,
                x.Language,
                x.AvailableBooks,
                x.AuthorId,
                x.PublisherId,
                x.LibraryId
            ))
            .ToListAsync(ct);

        return books.Any()
            ? MobileResponse<IEnumerable<BookDto>>.Success(books, "Fetched successfully.", "SUCCESS-200")
            : MobileResponse<IEnumerable<BookDto>>.EmptyValues(Enumerable.Empty<BookDto>());
    }

    public async Task<MobileResponse<BookDto>> GetBookByIdAsync(int id, CancellationToken ct)
    {
        var book = await _context.Books.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BookDto(
                x.Id, x.BookName, x.Title, x.Language,
                x.AvailableBooks, x.AuthorId, x.PublisherId, x.LibraryId
            ))
            .FirstOrDefaultAsync(ct);

        return book is null
            ? MobileResponse<BookDto>.Fail("Book not found.", "ERROR-404")
            : MobileResponse<BookDto>.Success(book, "Fetched successfully.", "SUCCESS-200");
    }

    public async Task<MobileResponse<BookDto>> CreateBookAsync(CreateBookDto dto, CancellationToken ct)
    {
        var entity = new Book
        {
            BookName = dto.BookName.Trim(),
            Title = dto.Title.Trim(),
            Language = dto.Language.Trim(),
            AvailableBooks = dto.AvailableBooks,
            LibraryId = dto.LibraryId,
            AuthorId = dto.AuthorId,
            PublisherId = dto.PublisherId
        };

        try
        {
            await _context.Books.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return MobileResponse<BookDto>.Success(
                new BookDto(entity.Id, entity.BookName, entity.Title, entity.Language, entity.AvailableBooks, entity.AuthorId, entity.PublisherId, entity.LibraryId),
                "Book created successfully.",
                "SUCCESS-201"
            );
        }
        catch (Exception ex)
        {
            return MobileResponse<BookDto>.Fail($"Failed to create book: {ex.Message}", "ERROR-500");
        }
    }

    public async Task<MobileResponse<BookDto>> UpdateBookAsync(UpdateBookDto dto, CancellationToken ct)
    {
        var entity = await _context.Books.FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

        if (entity is null)
            return MobileResponse<BookDto>.Fail("Book not found.", "ERROR-404");

        entity.BookName = dto.BookName.Trim();
        entity.Title = dto.Title.Trim();
        entity.Language = dto.Language.Trim();
        entity.AvailableBooks = dto.AvailableBooks;
        entity.AuthorId = dto.AuthorId;
        entity.PublisherId = dto.PublisherId;

        await _context.SaveChangesAsync(ct);

        return MobileResponse<BookDto>.Success(
            new BookDto(entity.Id, entity.BookName, entity.Title, entity.Language, entity.AvailableBooks, entity.AuthorId, entity.PublisherId, entity.LibraryId),
            "Book updated successfully.",
            "SUCCESS-200"
        );
    }

    public async Task<MobileResponse<bool>> DeleteBookAsync(int id, CancellationToken ct)
    {
        try
        {
            var entity = await _context.Books.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
                return MobileResponse<bool>.Fail("Book not found.", "ERROR-404");

            _context.Books.Remove(entity);
            await _context.SaveChangesAsync(ct);

            return MobileResponse<bool>.Success(true, "Book deleted successfully.", "SUCCESS-200");
        }
        catch (Exception ex)
        {
            return MobileResponse<bool>.ExceptionFailed($"Failed to delete book: {ex.Message}", "ERROR-500");
        }

    }
}
