using RailwayReservation.Application.Auth.Ports;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Application.Auth;

public sealed record LoginResult(string AccessToken, Guid UserId, string Email, string Name, UserRole Role);

public sealed class LoginUseCase(IUserReader userReader, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
{
    public async Task<LoginResult> ExecuteAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await userReader.FindByEmailAsync(email, ct);
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
        {
            throw new UnauthorizedAppException("INVALID_CREDENTIALS", "Invalid email or password.");
        }

        var token = jwtTokenService.IssueToken(user);
        return new LoginResult(token, user.Id, user.Email, user.Name, user.Role);
    }
}
