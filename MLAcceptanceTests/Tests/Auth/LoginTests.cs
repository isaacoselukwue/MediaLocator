namespace ML.AcceptanceTests.Tests.Auth;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class LoginTests : PageTest
{
    private LoginPage _loginPage;

    [SetUp]
    public async Task SetupAsync()
    {
        await Context.ClearCookiesAsync();
        _loginPage = new LoginPage(Browser, Page);
        await _loginPage.NavigateToAsync();
    }

    [Test]
    public async Task ShouldDisplayLoginForm()
    {
        await Expect(Page).ToHaveTitleAsync("Login - MediaLocator");

        await Expect(Page.Locator("#email")).ToBeVisibleAsync();
        await Expect(Page.Locator("#password")).ToBeVisibleAsync();
        await Expect(Page.Locator(".login-button")).ToBeVisibleAsync();
        await Expect(Page.Locator(".forgot-password a")).ToBeVisibleAsync();
        await Expect(Page.Locator("a[href='signup']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ShouldShowValidationErrorsOnEmptySubmit()
    {
        await _loginPage.ClickLoginAsync();

        await _loginPage.VerifyEmailValidationErrorAsync();
        await _loginPage.VerifyPasswordValidationErrorAsync();
    }

    [Test]
    public async Task ShouldShowValidationErrorForInvalidEmail()
    {
        await _loginPage.EnterEmailAsync("not-an-email");
        await _loginPage.EnterPasswordAsync("password123");
        await _loginPage.ClickLoginAsync();

        var validationMessageLocator = Page.Locator(".validation-message").First;
        var validationMessage = await validationMessageLocator.TextContentAsync();
        await Assertions.Expect(validationMessageLocator).ToContainTextAsync("Invalid email format");
    }

    [Test]
    public async Task ShouldShowLoadingStateWhenSubmitting()
    {
        await Page.RouteAsync(new Regex("/api/auth/login"), async route =>
        {
            await Task.Delay(500);
            await route.FulfillAsync(new()
            {
                Status = 200,
                Body = """{"succeeded":true,"data":{"accessToken":"fake-token"}}"""
            });
        });

        await _loginPage.EnterEmailAsync("test@example.com");
        await _loginPage.EnterPasswordAsync("password123");
        await _loginPage.ClickLoginAsync();

        await Expect(_loginPage.Page.Locator(".spinner-border")).ToBeVisibleAsync();
        await Expect(_loginPage.Page.Locator("button.login-button")).ToBeDisabledAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(".*/"));
    }

    [Test]
    public async Task ShouldShowErrorMessageOnLoginFailure()
    {
        await Page.RouteAsync(new Regex("/api/auth/login"), async route =>
        {
            await route.FulfillAsync(new()
            {
                Status = 401,
                ContentType = "application/json",
                Body = """{"succeeded":false,"message":"Invalid credentials","errors":["Invalid username or password"]}"""
            });
        });

        await _loginPage.LoginAsync("test@example.com", "wrong-password");

        await Expect(_loginPage.Page.Locator(".error-title")).ToBeVisibleAsync();
        await Expect(_loginPage.Page.Locator(".error-title")).ToContainTextAsync("Please check your credentials.");
        await Expect(_loginPage.Page.Locator(".error-details li")).ToContainTextAsync("Invalid username or password");
    }

    [Test]
    public async Task ShouldNavigateToForgotPasswordPage()
    {
        await _loginPage.ClickForgotPasswordAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(".*/forgot-password"));
    }

    [Test]
    public async Task ShouldNavigateToSignUpPage()
    {
        await _loginPage.ClickSignUpAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(".*/signup"));
    }
}