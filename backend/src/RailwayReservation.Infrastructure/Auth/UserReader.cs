using Microsoft.EntityFrameworkCore;
using RailwayReservation.Application.Auth.Ports;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Infrastructure.Auth;

public sealed class UserReader(AppDbContext db) : IUserReader
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
}
