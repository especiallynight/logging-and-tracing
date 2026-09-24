using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Clients;

namespace ProjectService.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectDbContext _context;
    private readonly UserClient _userClient;

    public ProjectsController(ProjectDbContext context, UserClient userClient)
    {
        _context = context;
        _userClient = userClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        var projects = await _context.Projects.ToListAsync();

        return Ok(projects);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project is null)
        {
            return NotFound($"Проект с ID {id} не найден.");
        }

        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(Project project)
    {
        if (string.IsNullOrWhiteSpace(project.Name))
        {
            return BadRequest("Название проекта обязательно.");
        }

        try
        {
            var userExists = await _userClient.UserExistsAsync(project.OwnerId);

            if (!userExists)
            {
                return BadRequest(
                    $"Пользователь с ID {project.OwnerId} не существует.");
            }
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "UserService недоступен.");
        }

        _context.Projects.Add(project);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProject),
            new { id = project.Id },
            project);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProject(int id, Project project)
    {
        if (id != project.Id)
        {
            return BadRequest("ID в URL и ID проекта не совпадают.");
        }

        var existingProject = await _context.Projects.FindAsync(id);

        if (existingProject is null)
        {
            return NotFound($"Проект с ID {id} не найден.");
        }

        existingProject.Name = project.Name;
        existingProject.Description = project.Description;
        existingProject.OwnerId = project.OwnerId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project is null)
        {
            return NotFound($"Проект с ID {id} не найден.");
        }

        _context.Projects.Remove(project);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}