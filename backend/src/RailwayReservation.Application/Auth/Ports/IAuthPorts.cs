using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Application.Auth.Ports;

public interface IUserReader
{
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IJwtTokenService
{
    string IssueToken(User user);
}
