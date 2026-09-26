using PropMate.Api.Models;

namespace PropMate.Api.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}