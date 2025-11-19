namespace LibraryManagementSystem.Entities.DTOs;

public record BookDto(int Id, string BookName, string Title, string Language, int AvailableBooks, int AuthorId, int PublisherId, Guid LibraryId);
public record CreateBookDto(string BookName, string Title, string Language, int AvailableBooks, int AuthorId, int PublisherId, Guid LibraryId);
public record UpdateBookDto(int Id, string BookName, string Title, string Language, int AvailableBooks, int AuthorId, int PublisherId, Guid LibraryId);
