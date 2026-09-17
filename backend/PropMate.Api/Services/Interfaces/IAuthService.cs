using PropMate.Api.DTOs.Auth;

namespace PropMate.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
}