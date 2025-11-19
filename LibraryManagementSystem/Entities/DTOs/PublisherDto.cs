namespace LibraryManagementSystem.Entities.DTOs;

public record PublisherDto(int Id, string Name, string Address, string ContactInfo, int PublishYear);
public record CreatePublisherDto(string Name, string Address, string ContactNo, int PublishYear);
public record UpdatePublisherDto(int Id, string Name, string Address, string ContactInfo, int PublishYear);