namespace Task_Management.Models;

public class CurrentTask
{
    public int task_id { get; set; }

    public string? task_name { get; set; }

    public string? task_description { get; set; }

    public DateOnly dateadded { get; set; }

    public DateOnly deadlinedate { get; set; }

    public bool iscompleted { get; set; }

    public int statusid { get; set; }

    public int priorityid { get; set; }

    public StatusTask? StatusTask { get; set; }

    public TaskPriority? TaskPriority { get; set; }
    public int? project_id { get; set; }

    public int? assigned_user_id { get; set; }
}