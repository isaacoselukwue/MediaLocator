using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using ML.Domain.Constants;
using ML.Domain.Enums;

namespace ML.Infrastructure.Data;


public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<MLDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}

public class MLDbContextInitialiser(ILogger<MLDbContextInitialiser> logger, MLDbContext context, UserManager<Users> userManager, RoleManager<UserRoles> roleManager)
{
    private readonly ILogger<MLDbContextInitialiser> _logger = logger;
    private readonly MLDbContext _context = context;
    private readonly UserManager<Users> _userManager = userManager;
    private readonly RoleManager<UserRoles> _roleManager = roleManager;

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        List<UserRoles> defaultRoles = [
            new UserRoles { Name = Roles.Admin, UserRoleStatus = StatusEnum.Active },
            new UserRoles { Name = Roles.User, UserRoleStatus = StatusEnum.Active }
            ];
        
        foreach (UserRoles role in defaultRoles)
        {
            if (_roleManager.Roles.All(r => r.Name != role.Name))
            {
                await _roleManager.CreateAsync(role);
            }
        }

        Users administrator = new()
        {
            Created = DateTimeOffset.Now,
            CreatedBy = "System",
            Email = "princeizak@live.com",
            EmailConfirmed = true,
            FirstName = "Isaac",
            LastModified = DateTimeOffset.Now,
            LastName = "Ose",
            PhoneNumber = "07000000000",
            PhoneNumberConfirmed = true,
            UsersStatus = StatusEnum.Active
        };
        

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            List<string> availableRoles = [.. defaultRoles.Select(r => r.Name).Where(roleName => !string.IsNullOrWhiteSpace(roleName))];
            await _userManager.AddToRolesAsync(administrator, availableRoles);
        }
    }
}