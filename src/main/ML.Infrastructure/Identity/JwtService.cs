﻿using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ML.Application.Authentication.Commands.Login;
using ML.Application.Common.Models;
using ML.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ML.Infrastructure.Identity;
internal class JwtService(ICurrentUser currentUser, IDataProtectionProvider dataProtectionProvider, IOptions<JwtSettings> jwtSettings) : IJwtService
{
    private readonly IDataProtectionProvider _dataProtectionProvider = dataProtectionProvider;
    private readonly JwtSettings jwtSettings = jwtSettings.Value;

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
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""),
            ..roles.Select(roles => new Claim(ClaimTypes.Role, roles)),
            ..userClaims
        ];

        JwtSecurityToken token = new(jwtSettings.Issuer, jwtSettings.Audience, claims, expires: expiry, signingCredentials: credentials);
        LoginDto loginDto = new()
        {
            AccessToken = new()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = GenerateProtectedToken(user.Id),
                ExpiresIn = (long)expiry.Subtract(creationDate.UtcDateTime).TotalSeconds
            }
        };

        return Result<LoginDto>.Success(ResultMessage.AccessTokenGenerated, loginDto);
    }
    private string GenerateProtectedToken(Guid userId)
    {
        string token = GenerateRefreshToken();
        token = string.Concat(token, "|", userId);
        IDataProtector protector = _dataProtectionProvider.CreateProtector("JwtProtector");
        ITimeLimitedDataProtector timeLimitProtector = protector.ToTimeLimitedDataProtector();
        string encryptedUserToken = timeLimitProtector.Protect(token, TimeSpan.FromHours(10));
        return encryptedUserToken;
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    public (string token, string userId) UnprotectToken(string protectedToken)
    {
        IDataProtector protector = _dataProtectionProvider.CreateProtector("JwtProtector");
        ITimeLimitedDataProtector timeLimitProtector = protector.ToTimeLimitedDataProtector();
        string token = timeLimitProtector.Unprotect(protectedToken);
        string[] tokenParts = token.Split('|');
        return (tokenParts[0], tokenParts[1]);
    }

    public Guid GetUserId()
    {
        Guid userId = currentUser.UserId;
        return userId;
    }

    public string GetEmailAddress()
    {
        string email = currentUser.Email;
        return email;
    }
}
