using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using ML.Application.Accounts.Commands.Login;
using ML.Application.Common.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ML.Infrastructure.Identity;
internal class JwtService(IDataProtectionProvider dataProtectionProvider, JwtSettings jwtSettings)
{
    private readonly IDataProtectionProvider _dataProtectionProvider = dataProtectionProvider;

    public Result<LoginDto> GenerateToken(Users users)
    {
        SymmetricSecurityKey key = new(Encoding.ASCII.GetBytes(jwtSettings.Secret!));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
        DateTime expiry = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes);
        DateTimeOffset creationDate = DateTimeOffset.UtcNow;
        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, users.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, creationDate.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Sid, Guid.CreateVersion7().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, users.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.GivenName, users.FirstName ?? ""),
            new Claim(JwtRegisteredClaimNames.FamilyName, users.LastName ?? "")
        ];
        JwtSecurityToken token = new(jwtSettings.Issuer, jwtSettings.Audience, claims, expires: expiry, signingCredentials: credentials);
        LoginDto loginDto = new()
        {
            AccessToken = new()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = GenerateProtectedToken(),
                ExpiresIn = (long)expiry.Subtract(creationDate.UtcDateTime).TotalSeconds
            }
        };

        return Result<LoginDto>.Success("Access token generated successfully.", loginDto);
    }
    private string GenerateProtectedToken()
    {
        string token = GenerateRefreshToken();
        IDataProtector protector = _dataProtectionProvider.CreateProtector("JwtProtector");
        ITimeLimitedDataProtector timeLimitProtector = protector.ToTimeLimitedDataProtector();
        string encryptedUserToken = timeLimitProtector.Protect(token, TimeSpan.FromMinutes(10));
        return encryptedUserToken;
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
