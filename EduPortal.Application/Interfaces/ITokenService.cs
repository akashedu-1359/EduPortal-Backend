namespace EduPortal.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, string role, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    string HashToken(string token);
    bool ValidateAccessToken(string token, out Guid userId);
}
