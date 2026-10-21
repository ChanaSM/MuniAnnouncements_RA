using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MuniAnnouncements.Api.Auth;

public static class DevTokenEndpoint
{
    public static void MapDevTokenEndpoint(this WebApplication app)
    {
        app.MapPost("/api/dev/token", (string idNumber, IOptions<JwtOptions> options) =>
        {
            var jwt = options.Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));

            var token = new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: new[] { new Claim(JwtOptions.IdNumberClaim, idNumber) },
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        })
        .AllowAnonymous()
        .WithTags("Dev only");
    }
}
