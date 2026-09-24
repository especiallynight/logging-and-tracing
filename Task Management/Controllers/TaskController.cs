using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Management.Clients;
using Task_Management.Data;
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

    public TasksController(TaskManagementDbContext context, ILogger<TasksController> logger, UserClient userClient, ProjectClient projectClient)
    {
        _context = context;
        _logger = logger;
        _userClient = userClient;
        _projectClient = projectClient;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CurrentTask>>> GetTasks(
        [FromQuery] int? statusid = null)
    {
        _logger.LogInformation(
            "Получение списка задач. StatusId: {StatusId}",
            statusid);

        var query = _context.CurrentTasks.AsQueryable();

        if (statusid.HasValue)
        {
            query = query.Where(task =>
                task.statusid == statusid.Value);
        }

        var tasks = await query.ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CurrentTask>> GetTask(int id)
    {
        _logger.LogInformation(
            "Получение задачи с ID {TaskId}",
            id);

        var task = await _context.CurrentTasks
            .FirstOrDefaultAsync(x => x.task_id == id);

        if (task is null)
        {
            _logger.LogWarning(
                "Задача с ID {TaskId} не найдена",
                id);

            return NotFound(
                $"Задача с ID {id} не найдена.");
        }

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<CurrentTask>> CreateTask(
        CurrentTask task)
    {
        _logger.LogInformation(
            "Создание новой задачи: {TaskName}",
            task.task_name);

        var statusExists = await _context.StatusTasks
            .AnyAsync(x => x.IdTaskStatus == task.statusid);

        if (!statusExists)
        {
            return BadRequest(
                $"Статус с ID {task.statusid} не существует.");
        }

        var priorityExists = await _context.TaskPriorities
            .AnyAsync(x => x.IdPriority == task.priorityid);

        if (!priorityExists)
        {
            return BadRequest(
                $"Приоритет с ID {task.priorityid} не существует.");
        }

        if (task.assigned_user_id.HasValue)
        {
            try
            {
                var userExists = await _userClient.UserExistsAsync(
                    task.assigned_user_id.Value);

                if (!userExists)
                {
                    return BadRequest(
                        $"Пользователь с ID {task.assigned_user_id} не существует.");
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    "UserService недоступен.");
            }
        }

        if (task.project_id.HasValue)
        {
            try
            {
                var projectExists = await _projectClient.ProjectExistsAsync(
                    task.project_id.Value);

                if (!projectExists)
                {
                    return BadRequest(
                        $"Проект с ID {task.project_id} не существует.");
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    "ProjectService недоступен.");
            }
        }

        _context.CurrentTasks.Add(task);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Задача создана. ID: {TaskId}",
            task.task_id);

        return CreatedAtAction(
            nameof(GetTask),
            new { id = task.task_id },
            task);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTask(
        int id,
        CurrentTask task)
    {
        if (id != task.task_id)
        {
            return BadRequest(
                "ID в URL и ID задачи не совпадают.");
        }

        var existingTask = await _context.CurrentTasks
            .FirstOrDefaultAsync(x => x.task_id == id);

        if (existingTask is null)
        {
            _logger.LogWarning(
                "Попытка изменить несуществующую задачу {TaskId}",
                id);

            return NotFound(
                $"Задача с ID {id} не найдена.");
        }

        var statusExists = await _context.StatusTasks
            .AnyAsync(x => x.IdTaskStatus == task.statusid);

        if (!statusExists)
        {
            return BadRequest(
                $"Статус с ID {task.statusid} не существует.");
        }

        var priorityExists = await _context.TaskPriorities
            .AnyAsync(x => x.IdPriority == task.priorityid);

        if (!priorityExists)
        {
            return BadRequest(
                $"Приоритет с ID {task.priorityid} не существует.");
        }
        if (task.assigned_user_id.HasValue)
        {
            try
            {
                var userExists = await _userClient.UserExistsAsync(task.assigned_user_id.Value);

                if (!userExists)
                {
                    return BadRequest(
                        $"Пользователь с ID {task.assigned_user_id} не существует.");
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    "UserService недоступен.");
            }
        }

        if (task.project_id.HasValue)
        {
            try
            {
                var projectExists = await _projectClient.ProjectExistsAsync(task.project_id.Value);

                if (!projectExists)
                {
                    return BadRequest($"Проект с ID {task.project_id} не существует.");
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    "ProjectService недоступен.");
            }
        }

        existingTask.task_name = task.task_name;
        existingTask.task_description = task.task_description;
        existingTask.dateadded = task.dateadded;
        existingTask.deadlinedate = task.deadlinedate;
        existingTask.iscompleted = task.iscompleted;
        existingTask.statusid = task.statusid;
        existingTask.priorityid = task.priorityid;
        existingTask.project_id = task.project_id;
        existingTask.assigned_user_id = task.assigned_user_id;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Задача {TaskId} обновлена",
            id);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        _logger.LogInformation(
            "Удаление задачи с ID {TaskId}",
            id);

        var task = await _context.CurrentTasks
            .FirstOrDefaultAsync(x => x.task_id == id);

        if (task is null)
        {
            _logger.LogWarning(
                "Попытка удалить несуществующую задачу {TaskId}",
                id);

            return NotFound(
                $"Задача с ID {id} не найдена.");
        }

        _context.CurrentTasks.Remove(task);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Задача {TaskId} удалена",
            id);

        return NoContent();
    }

    [HttpPost("archive")]
    public async Task<IActionResult> ArchiveTasks()
    {
        _logger.LogInformation("Запуск архивации задач");

        await _context.Database.ExecuteSqlRawAsync(
            "SELECT archiveTask()");

        _logger.LogInformation(
            "Архивация задач завершена");

        return Ok("Задачи успешно архивированы.");
    }

    [HttpGet("archive")]
    public async Task<ActionResult<IEnumerable<ArchivedTask>>>
        GetArchivedTasks()
    {
        _logger.LogInformation(
            "Получение архивных задач");

        var archivedTasks = await _context.Database
            .SqlQuery<ArchivedTask>($"""
                SELECT
                    idarchivedtask AS "IdArchivedTask",
                    task_id AS "TaskID",
                    completiondate AS "CompletionDate",
                    task_name AS "TaskName"
                FROM taskarchive
                ORDER BY completiondate DESC,
                         idarchivedtask DESC
                """)
            .ToListAsync();

        return Ok(archivedTasks);
    }
}