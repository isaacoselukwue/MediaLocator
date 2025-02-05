using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using ML.Application.Accounts.Commands.Login;
using ML.Application.Common.Models;
using ML.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ML.Infrastructure.Identity;
internal class JwtService(IDataProtectionProvider dataProtectionProvider, JwtSettings jwtSettings) : IJwtService
{
    private readonly IDataProtectionProvider _dataProtectionProvider = dataProtectionProvider;

    public Result<LoginDto> GenerateToken(Users user, List<Claim> userClaims, List<string> roles)
    {
        SymmetricSecurityKey key = new(Encoding.ASCII.GetBytes(jwtSettings.Secret!));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
        DateTime expiry = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes);
        DateTimeOffset creationDate = DateTimeOffset.UtcNow;
        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, creationDate.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Sid, Guid.CreateVersion7().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? ""),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? "")
        ];
        claims.AddRange(userClaims);
        foreach(string role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

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

        return Result<LoginDto>.Success(ResultMessage.AccessTokenGenerated, loginDto);
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
