namespace Task_Management.DTOs;

public record CreateTaskDto(
    string? TaskName,
    string? TaskDescription,
    DateOnly DateAdded,
    DateOnly DeadlineDate,
    bool IsCompleted,
    int StatusId,
    int PriorityId,
    int? ProjectId,
    int? AssignedUserId);