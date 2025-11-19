namespace LibraryManagementSystem.Entities.DTOs;

public record LibraryDto(Guid Id, string LibraryName, string Location, string ContactNo, string UserId);
public record CreateLibraryDto
{
    public string LibraryName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ContactNo { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
public record UpdateLibraryDto(Guid Id, string LibraryName, string Location, string ContactNo);
