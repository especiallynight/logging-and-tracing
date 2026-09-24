namespace UserService.DTOs;

public record UpdateUserDto(string UserName, string Email, string? Password);