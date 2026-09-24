using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectService.Clients;
using ProjectService.Data;
using ProjectService.DTOs;
using ProjectService.Models;

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
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
    {
        var projects = await _context.Projects.ToListAsync();
        return Ok(projects.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project is null)
        {
            return NotFound($"Проект с ID {id} не найден.");
        }

        return Ok(ToDto(project));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Название проекта обязательно.");
        }

        try
        {
            var userExists = await _userClient.UserExistsAsync(dto.OwnerId);

            if (!userExists)
            {
                return BadRequest(
                    $"Пользователь с ID {dto.OwnerId} не существует.");
            }
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "UserService недоступен.");
        }

        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = dto.OwnerId
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProject),
            new { id = project.Id },
            ToDto(project));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto dto)
    {
        var existingProject = await _context.Projects.FindAsync(id);

        if (existingProject is null)
        {
            return NotFound($"Проект с ID {id} не найден.");
        }

        existingProject.Name = dto.Name;
        existingProject.Description = dto.Description;
        existingProject.OwnerId = dto.OwnerId;

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

    private static ProjectDto ToDto(Project p)
        => new(p.Id, p.Name, p.Description, p.OwnerId);
}