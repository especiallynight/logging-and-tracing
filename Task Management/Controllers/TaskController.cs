using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Management.Clients;
using Task_Management.Data;
using Task_Management.DTOs;
using Task_Management.Models;
using TaskManagement.Clients;

namespace Task_Management.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<TasksController> _logger;
    private readonly UserClient _userClient;
    private readonly ProjectClient _projectClient;

    public TasksController(
        TaskManagementDbContext context,
        ILogger<TasksController> logger,
        UserClient userClient,
        ProjectClient projectClient)
    {
        _context = context;
        _logger = logger;
        _userClient = userClient;
        _projectClient = projectClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks(
        [FromQuery] int? statusid = null)
    {
        _logger.LogInformation(
            "Получение списка задач. StatusId: {StatusId}",
            statusid);

        var query = _context.CurrentTasks
            .Include(t => t.StatusTask)
            .Include(t => t.TaskPriority)
            .AsQueryable();

        if (statusid.HasValue)
        {
            query = query.Where(task => task.statusid == statusid.Value);
        }

        var tasks = await query.ToListAsync();
        return Ok(tasks.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        _logger.LogInformation("Получение задачи с ID {TaskId}", id);

        var task = await _context.CurrentTasks
            .Include(t => t.StatusTask)
            .Include(t => t.TaskPriority)
            .FirstOrDefaultAsync(x => x.task_id == id);

        if (task is null)
        {
            _logger.LogWarning("Задача с ID {TaskId} не найдена", id);
            return NotFound($"Задача с ID {id} не найдена.");
        }

        return Ok(ToDto(task));
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto dto)
    {
        _logger.LogInformation("Создание новой задачи: {TaskName}", dto.TaskName);

        var statusExists = await _context.StatusTasks
            .AnyAsync(x => x.IdTaskStatus == dto.StatusId);

        if (!statusExists)
        {
            return BadRequest($"Статус с ID {dto.StatusId} не существует.");
        }

        var priorityExists = await _context.TaskPriorities
            .AnyAsync(x => x.IdPriority == dto.PriorityId);

        if (!priorityExists)
        {
            return BadRequest($"Приоритет с ID {dto.PriorityId} не существует.");
        }

        if (dto.AssignedUserId.HasValue)
        {
            try
            {
                var userExists = await _userClient.UserExistsAsync(dto.AssignedUserId.Value);
                if (!userExists)
                {
                    return BadRequest($"Пользователь с ID {dto.AssignedUserId} не существует.");
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "UserService недоступен.");
            }
        }

        if (dto.ProjectId.HasValue)
        {
            try
            {
                var projectExists = await _projectClient.ProjectExistsAsync(dto.ProjectId.Value);
                if (!projectExists)
                {
                    return BadRequest($"Проект с ID {dto.ProjectId} не существует.");
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "ProjectService недоступен.");
            }
        }

        var task = new CurrentTask
        {
            task_name = dto.TaskName,
            task_description = dto.TaskDescription,
            dateadded = dto.DateAdded,
            deadlinedate = dto.DeadlineDate,
            iscompleted = dto.IsCompleted,
            statusid = dto.StatusId,
            priorityid = dto.PriorityId,
            project_id = dto.ProjectId,
            assigned_user_id = dto.AssignedUserId
        };

        _context.CurrentTasks.Add(task);
        await _context.SaveChangesAsync();

        await _context.Entry(task).Reference(t => t.StatusTask).LoadAsync();
        await _context.Entry(task).Reference(t => t.TaskPriority).LoadAsync();

        _logger.LogInformation("Задача создана. ID: {TaskId}", task.task_id);

        return CreatedAtAction(
            nameof(GetTask),
            new { id = task.task_id },
            ToDto(task));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto dto)
    {
        var existingTask = await _context.CurrentTasks
            .FirstOrDefaultAsync(x => x.task_id == id);

        if (existingTask is null)
        {
            _logger.LogWarning("Попытка изменить несуществующую задачу {TaskId}", id);
            return NotFound($"Задача с ID {id} не найдена.");
        }

        var statusExists = await _context.StatusTasks
            .AnyAsync(x => x.IdTaskStatus == dto.StatusId);

        if (!statusExists)
        {
            return BadRequest($"Статус с ID {dto.StatusId} не существует.");
        }

        var priorityExists = await _context.TaskPriorities
            .AnyAsync(x => x.IdPriority == dto.PriorityId);

        if (!priorityExists)
        {
            return BadRequest($"Приоритет с ID {dto.PriorityId} не существует.");
        }

        if (dto.AssignedUserId.HasValue)
        {
            try
            {
                var userExists = await _userClient.UserExistsAsync(dto.AssignedUserId.Value);
                if (!userExists)
                {
                    return BadRequest($"Пользователь с ID {dto.AssignedUserId} не существует.");
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "UserService недоступен.");
            }
        }

        if (dto.ProjectId.HasValue)
        {
            try
            {
                var projectExists = await _projectClient.ProjectExistsAsync(dto.ProjectId.Value);
                if (!projectExists)
                {
                    return BadRequest($"Проект с ID {dto.ProjectId} не существует.");
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "ProjectService недоступен.");
            }
        }

        existingTask.task_name = dto.TaskName;
        existingTask.task_description = dto.TaskDescription;
        existingTask.dateadded = dto.DateAdded;
        existingTask.deadlinedate = dto.DeadlineDate;
        existingTask.iscompleted = dto.IsCompleted;
        existingTask.statusid = dto.StatusId;
        existingTask.priorityid = dto.PriorityId;
        existingTask.project_id = dto.ProjectId;
        existingTask.assigned_user_id = dto.AssignedUserId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Задача {TaskId} обновлена", id);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        _logger.LogInformation("Удаление задачи с ID {TaskId}", id);

        var task = await _context.CurrentTasks
            .FirstOrDefaultAsync(x => x.task_id == id);

        if (task is null)
        {
            _logger.LogWarning("Попытка удалить несуществующую задачу {TaskId}", id);
            return NotFound($"Задача с ID {id} не найдена.");
        }

        _context.CurrentTasks.Remove(task);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Задача {TaskId} удалена", id);

        return NoContent();
    }

    [HttpPost("archive")]
    public async Task<IActionResult> ArchiveTasks()
    {
        _logger.LogInformation("Запуск архивации задач");

        await _context.Database.ExecuteSqlRawAsync("SELECT archiveTask()");

        _logger.LogInformation("Архивация задач завершена");

        return Ok("Задачи успешно архивированы.");
    }

    [HttpGet("archive")]
    public async Task<ActionResult<IEnumerable<ArchivedTaskDto>>> GetArchivedTasks()
    {
        _logger.LogInformation("Получение архивных задач");

        var archivedTasks = await _context.Database
            .SqlQuery<ArchivedTask>($"""
                SELECT
                    idarchivedtask AS "IdArchivedTask",
                    task_id AS "TaskID",
                    completiondate AS "CompletionDate",
                    task_name AS "TaskName"
                FROM taskarchive
                ORDER BY completiondate DESC, idarchivedtask DESC
                """)
            .ToListAsync();

        return Ok(archivedTasks.Select(a => new ArchivedTaskDto(
            a.IdArchivedTask, a.TaskID, a.CompletionDate, a.TaskName)));
    }
    private static TaskDto ToDto(CurrentTask t)
        => new(
            t.task_id,
            t.task_name,
            t.task_description,
            t.dateadded,
            t.deadlinedate,
            t.iscompleted,
            t.statusid,
            t.StatusTask?.StatusName ?? string.Empty,
            t.priorityid,
            t.TaskPriority?.PriorityType ?? string.Empty,
            t.project_id,
            t.assigned_user_id);
}