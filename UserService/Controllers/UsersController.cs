using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
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
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null)
        {
            return NotFound($"Пользователь с ID {id} не найден.");
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        if (string.IsNullOrWhiteSpace(user.UserName))
        {
            return BadRequest("Имя пользователя обязательно.");
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return BadRequest("Email обязателен.");
        }

        var emailExists = await _context.Users.AnyAsync(x => x.Email == user.Email);

        if (emailExists)
        {
            return Conflict("Пользователь с таким email уже существует.");
        }

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id },
            user);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, User user)
    {
        if (id != user.Id)
        {
            return BadRequest("ID в URL и ID пользователя не совпадают.");
        }

        var existingUser = await _context.Users.FindAsync(id);

        if (existingUser is null)
        {
            return NotFound($"Пользователь с ID {id} не найден.");
        }

        existingUser.UserName = user.UserName;
        existingUser.Email = user.Email;

        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            existingUser.PasswordHash = user.PasswordHash;
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
            return NotFound(
                $"Пользователь с ID {id} не найден.");
        }

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}