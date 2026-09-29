using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.DTOs.Auth;
using PropMate.Api.Models;
using PropMate.Api.Services.Interfaces;
using PropMate.Api.Enums;

namespace PropMate.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(
        AppDbContext context,
        IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLower();

        var emailExists = await _context.Users
            .AnyAsync(x => x.Email == email);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = email,
            Role = UserRole.BuyerRenter,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            Token = _jwtService.GenerateToken(user)
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLower();

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            Token = _jwtService.GenerateToken(user)
        };
    }
}