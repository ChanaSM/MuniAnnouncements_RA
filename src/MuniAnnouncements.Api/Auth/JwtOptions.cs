namespace MuniAnnouncements.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const string IdNumberClaim = "idNumber";

    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string SigningKey { get; init; }
}
