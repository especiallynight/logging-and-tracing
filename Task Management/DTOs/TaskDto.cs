namespace Task_Management.DTOs;

public record TaskDto(
    int TaskId,
    string? TaskName,
    string? TaskDescription,
    DateOnly DateAdded,
    DateOnly DeadlineDate,
    bool IsCompleted,
    int StatusId,
    string StatusName,
    int PriorityId,
    string PriorityName,
    int? ProjectId,
    int? AssignedUserId);