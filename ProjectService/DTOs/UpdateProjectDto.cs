namespace ProjectService.DTOs;

public record UpdateProjectDto(
    string Name,
    string? Description,
    int OwnerId);