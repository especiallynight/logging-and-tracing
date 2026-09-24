namespace Task_Management.DTOs;

public record ArchivedTaskDto(
    int IdArchivedTask,
    int TaskID,
    DateOnly CompletionDate,
    string TaskName);