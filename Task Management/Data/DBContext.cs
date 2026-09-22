using Microsoft.EntityFrameworkCore;
using Task_Management.Models;

namespace Task_Management.Data;

public class TaskManagementDbContext : DbContext
{
    public TaskManagementDbContext(
        DbContextOptions<TaskManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<CurrentTask> CurrentTasks => Set<CurrentTask>();
    public DbSet<StatusTask> StatusTasks => Set<StatusTask>();
    public DbSet<TaskPriority> TaskPriorities => Set<TaskPriority>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CurrentTask>(entity =>
        {
            entity.ToTable("CurrentTasks");

            entity.HasKey(x => x.task_id);

            entity.Property(x => x.task_id)
                .HasColumnName("task_id");

            entity.Property(x => x.task_name)
                .HasColumnName("task_name");

            entity.Property(x => x.task_description)
                .HasColumnName("task_description");

            entity.Property(x => x.dateadded)
                .HasColumnName("dateadded");

            entity.Property(x => x.deadlinedate)
                .HasColumnName("deadlinedate");

            entity.Property(x => x.iscompleted)
                .HasColumnName("iscompleted");

            entity.Property(x => x.statusid)
                .HasColumnName("statusid");

            entity.Property(x => x.priorityid)
                .HasColumnName("priorityid");
            
            entity.Property(x => x.project_id)
                .HasColumnName("project_id");

            entity.Property(x => x.assigned_user_id)
                .HasColumnName("assigned_user_id");

            entity.HasOne(x => x.StatusTask)
                .WithMany(x => x.CurrentTasks)
                .HasForeignKey(x => x.statusid);

            entity.HasOne(x => x.TaskPriority)
                .WithMany(x => x.CurrentTasks)
                .HasForeignKey(x => x.priorityid);
        });

        modelBuilder.Entity<StatusTask>(entity =>
        {
            entity.ToTable("TaskStatuses");

            entity.HasKey(x => x.IdTaskStatus);

            entity.Property(x => x.IdTaskStatus)
                .HasColumnName("IdTaskStatus");

            entity.Property(x => x.StatusName)
                .HasColumnName("StatusName");
        });

        modelBuilder.Entity<TaskPriority>(entity =>
        {
            entity.ToTable("TaskPriorities");

            entity.HasKey(x => x.IdPriority);

            entity.Property(x => x.IdPriority)
                .HasColumnName("IdPriority");

            entity.Property(x => x.PriorityType)
                .HasColumnName("PriorityType");
        });
    }
}