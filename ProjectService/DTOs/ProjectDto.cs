namespace ProjectService.DTOs;

public record ProjectDto(
    int Id,
    string Name,
    string? Description,
    int OwnerId);