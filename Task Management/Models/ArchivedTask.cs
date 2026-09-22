namespace Task_Management.Models;

public class ArchivedTask
{
    public int IdArchivedTask { get; set; }

    public int TaskID { get; set; }

    public DateOnly CompletionDate { get; set; }

    public string TaskName { get; set; } = string.Empty;
}