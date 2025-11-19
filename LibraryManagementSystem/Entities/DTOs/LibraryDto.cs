namespace LibraryManagementSystem.Entities.DTOs;

public record LibraryDto(Guid Id, string LibraryName, string Location, string ContactNo, string UserId);
public record CreateLibraryDto(string LibraryName, string Location, string ContactNo, string UserId);
public record UpdateLibraryDto(Guid Id, string LibraryName, string Location, string ContactNo);
