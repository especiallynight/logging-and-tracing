using System.ComponentModel.DataAnnotations;

namespace Task_Management.Models
{
    public class StatusTask
    {
        [Key]
        public int IdTaskStatus { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public ICollection<CurrentTask> CurrentTasks { get; set; } = new List<CurrentTask>();
    }
}
