namespace LibraryManagementSystem.Entities.DTOs;

public record AuthorDto(int Id, string Name, string Biography, DateTime DateCreated);
public record CreateAuthorDto(string Name, string Biography);
public record UpdateAuthorDto(int Id, string Name, string Biography);

