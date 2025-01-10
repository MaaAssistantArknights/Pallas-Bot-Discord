using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace PallasBot.Domain.Utils;

public static class JwtUtils
{
    public static async Task<string> GenerateGitHubAppJwtAsync(string clientId, string pemFile)
    {
        var pemContent = await File.ReadAllTextAsync(pemFile);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(pemContent);

        var handler = new JwtSecurityTokenHandler();

        var now = DateTime.UtcNow;

        var descriptor = new SecurityTokenDescriptor
        {
            IssuedAt = now,
            Expires = now.AddMinutes(10),
            Issuer = clientId,
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), "RS256")
        };

        var token = handler.CreateEncodedJwt(descriptor)
                    ?? throw new InvalidOperationException("Failed to create JWT token");

        return token;
    }
}
