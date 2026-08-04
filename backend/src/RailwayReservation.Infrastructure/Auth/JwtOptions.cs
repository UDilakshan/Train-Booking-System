namespace RailwayReservation.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = default!;
    public int ExpiresInMinutes { get; set; } = 480; // 8h, matches the original Node build's default
    public string Issuer { get; set; } = "railway-reservation";
}
