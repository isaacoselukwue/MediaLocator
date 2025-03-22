using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ML.Infrastructure.Identity;

namespace ML.Application.FunctionalTests.Accounts.Services;

[TestFixture]
class IdentityServiceTests
{
    private Mock<UserManager<Users>> _mockUserManager;
    private Mock<SignInManager<Users>> _mockSignInManager;
    private Mock<IJwtService> _mockJwtService;
    private Mock<IMLDbContext> _mockDbContext;
    private IdentityService _identityService;
    private Guid _validUserId;
    private Guid _invalidUserId;
    private Guid _alreadyActiveUserId;
    private Guid _updateFailUserId;
    private string _currentUserEmail;
    private Users _validUser;
    private Users _alreadyActiveUser;
    private Users _updateFailUser;

    [SetUp]
    public void Setup()
    {
        _validUserId = Guid.NewGuid();
        _invalidUserId = Guid.NewGuid();
        _alreadyActiveUserId = Guid.NewGuid();
        _updateFailUserId = Guid.NewGuid();
        _currentUserEmail = "current@example.com";

        _mockUserManager = MockUserManager();
        _mockSignInManager = MockSignInManager();
        _mockJwtService = new Mock<IJwtService>();
        _mockDbContext = new Mock<IMLDbContext>();

        _validUser = new Users
        {
            Id = _validUserId,
            Email = "valid@example.com",
            UsersStatus = StatusEnum.InActive
        };

        _alreadyActiveUser = new Users
        {
            Id = _alreadyActiveUserId,
            Email = "active@example.com",
            UsersStatus = StatusEnum.Active
        };

        _updateFailUser = new Users
        {
            Id = _updateFailUserId,
            Email = "fail@example.com",
            UsersStatus = StatusEnum.InActive
        };

        _mockJwtService.Setup(x => x.GetEmailAddress()).Returns(_currentUserEmail);

        _mockUserManager.Setup(x => x.FindByIdAsync(_validUserId.ToString()))
            .ReturnsAsync(_validUser);

        _mockUserManager.Setup(x => x.FindByIdAsync(_alreadyActiveUserId.ToString()))
            .ReturnsAsync(_alreadyActiveUser);

        _mockUserManager.Setup(x => x.FindByIdAsync(_updateFailUserId.ToString()))
            .ReturnsAsync(_updateFailUser);

        _mockUserManager.Setup(x => x.FindByIdAsync(_invalidUserId.ToString()))
            .ReturnsAsync((Users)null);

        _mockUserManager.Setup(x => x.UpdateAsync(_validUser))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.UpdateAsync(_updateFailUser))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

        _identityService = new(_mockSignInManager.Object, _mockUserManager.Object, _mockJwtService.Object, _mockDbContext.Object);
    }

    [Test]
    public async Task ActivateAccount_UserNotFound_ReturnsFailed()
    {
        var result = await _identityService.ActivateAccountAsync(_invalidUserId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Item1.Succeeded, Is.False);
            Assert.That(result.Item1.Errors, Contains.Item("Invalid user"));
            Assert.That(result.email, Is.Empty);
        });
    }

    [Test]
    public async Task ActivateAccount_AlreadyActive_ReturnsFailed()
    {
        var result = await _identityService.ActivateAccountAsync(_alreadyActiveUserId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Item1.Succeeded, Is.False);
            Assert.That(result.Item1.Errors, Contains.Item("Account is already active"));
            Assert.That(result.email, Is.Empty);
        });
    }

    [Test]
    public async Task ActivateAccount_UpdateFails_ReturnsFailed()
    {
        var result = await _identityService.ActivateAccountAsync(_updateFailUserId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Item1.Succeeded, Is.False);
            Assert.That(result.Item1.Errors, Contains.Item("Update failed"));
            Assert.That(result.email, Is.Empty);
        });
    }

    [Test]
    public async Task ActivateAccount_Success_ReturnsSuccessWithEmail()
    {
        var result = await _identityService.ActivateAccountAsync(_validUserId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Item1.Succeeded, Is.True);
            Assert.That(result.Item1.Message, Is.EqualTo(ResultMessage.ActivateAccountSuccess));
            Assert.That(result.email, Is.EqualTo(_validUser.Email));
            Assert.That(_validUser.UsersStatus, Is.EqualTo(StatusEnum.Active));
            Assert.That(_validUser.LastModifiedBy, Is.EqualTo(_currentUserEmail));
        });

        _mockUserManager.Verify(x => x.UpdateAsync(_validUser), Times.Once);
    }

    private static Mock<UserManager<Users>> MockUserManager()
    {
        return new Mock<UserManager<Users>>(
            new Mock<IUserStore<Users>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<Users>>().Object,
            new IUserValidator<Users>[0],
            new IPasswordValidator<Users>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<Users>>>().Object);
    }

    private static Mock<SignInManager<Users>> MockSignInManager()
    {
        return new Mock<SignInManager<Users>>(
            new Mock<UserManager<Users>>(
                new Mock<IUserStore<Users>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<Users>>().Object,
                new IUserValidator<Users>[0],
                new IPasswordValidator<Users>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<Users>>>().Object).Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<Users>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<Users>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<Users>>().Object);
    }
}
