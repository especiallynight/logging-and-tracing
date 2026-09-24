using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _context;

    public UsersController(UserDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();

        return Ok(users.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null)
        {
            return NotFound($"Пользователь с ID {id} не найден.");
        }

        return Ok(ToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserName))
        {
            return BadRequest("Имя пользователя обязательно.");
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest("Email обязателен.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Пароль обязателен.");
        }

        var emailExists = await _context.Users.AnyAsync(x => x.Email == dto.Email);

        if (emailExists)
        {
            return Conflict("Пользователь с таким email уже существует.");
        }

        var user = new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PasswordHash = dto.Password
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id },
            ToDto(user));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        var existingUser = await _context.Users.FindAsync(id);

        if (existingUser is null)
        {
            return NotFound($"Пользователь с ID {id} не найден.");
        }

        existingUser.UserName = dto.UserName;
        existingUser.Email = dto.Email;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            existingUser.PasswordHash = dto.Password;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null)
        {
            return NotFound($"Пользователь с ID {id} не найден.");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static UserDto ToDto(User user)
        => new(user.Id, user.UserName, user.Email);
}