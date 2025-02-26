namespace ML.IntegrationTests.Test.Authentications;

using ML.Application.Authentication.Commands.Login;
using ML.Domain.Entities;
using ML.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;


public class LoginTests : BaseIntegrationTest
{
    private UserManager<Users> _userManager;
    private const string TestPassword = "Test@Password123!";

    [SetUp]
    public void Setup()
    {
        _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
        CleanupTestUsers().GetAwaiter().GetResult();
    }

    [Test]
    public async Task SignIn_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var testUser = await CreateTestUser();
        var command = new LoginCommand
        {
            EmailAddress = testUser.Email,
            Password = TestPassword
        };

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.AccessToken, Is.Not.Null);
        Assert.That(result.Data.AccessToken.AccessToken, Is.Not.Null);
        Assert.That(result.Data.AccessToken.RefreshToken, Is.Not.Null);
    }

    [Test]
    public async Task SignIn_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        var testUser = await CreateTestUser();
        var command = new LoginCommand
        {
            EmailAddress = testUser.Email,
            Password = "WrongPassword123!"
        };

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        //Assert.That(result.Errors, Is.EqualTo(ErrorType.Authentication));
    }

    [Test]
    public async Task SignIn_WithNonExistentUser_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand
        {
            EmailAddress = "nonexistent@example.com",
            Password = TestPassword
        };

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        //Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Authentication));
    }

    [Test]
    public async Task SignIn_WithLockedAccount_ShouldFail()
    {
        // Arrange
        var testUser = await CreateTestUser(lockout: true);
        var command = new LoginCommand
        {
            EmailAddress = testUser.Email,
            Password = TestPassword
        };

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.That(result.Succeeded, Is.False);
    }

    private async Task<Users> CreateTestUser(bool lockout = false)
    {
        var user = new Users
        {
            UserName = $"test-{Guid.NewGuid()}@example.com",
            Email = $"test-{Guid.NewGuid()}@example.com",
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User",
            UsersStatus = StatusEnum.Active,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(user, TestPassword);
        Assert.That(result.Succeeded, Is.True, "Failed to create test user");

        if (lockout)
        {
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddDays(1);
            await _userManager.UpdateAsync(user);
        }

        return user;
    }

    private async Task CleanupTestUsers()
    {
        var testUsers = await DbContext.Users
            .Where(u => u.Email != null && u.Email.StartsWith("test-"))
            .ToListAsync();

        foreach (var user in testUsers)
        {
            await _userManager.DeleteAsync(user);
        }

        await DbContext.SaveChangesAsync();
    }
}
