namespace ProjectService.DTOs;

public record CreateProjectDto(
    string Name,
    string? Description,
    int OwnerId);