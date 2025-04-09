using Microsoft.Playwright;
using ML.AcceptanceTests.Pages.Auth;

namespace ML.AcceptanceTests.Tests.Auth;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class SignupTests : PageTest
{
    private SignupPage _signupPage;

    [SetUp]
    public async Task SetupAsync()
    {
        await Context.ClearCookiesAsync();
        _signupPage = new SignupPage(Browser, Page);
        await _signupPage.NavigateToAsync();
    }

    [Test]
    public async Task ShouldDisplaySignupForm()
    {
        await Expect(Page).ToHaveTitleAsync("Sign Up - MediaLocator");

        await Expect(Page.Locator("#firstName")).ToBeVisibleAsync();
        await Expect(Page.Locator("#lastName")).ToBeVisibleAsync();
        await Expect(Page.Locator("#email")).ToBeVisibleAsync();
        await Expect(Page.Locator("#phoneNumber")).ToBeVisibleAsync();
        await Expect(Page.Locator("#password")).ToBeVisibleAsync();
        await Expect(Page.Locator("#confirmPassword")).ToBeVisibleAsync();
        await Expect(Page.Locator(".login-button")).ToBeVisibleAsync();
        await Expect(Page.Locator("a[href='login']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ShouldShowValidationErrorsOnEmptySubmit()
    {
        await _signupPage.ClickSignUpAsync();

        await _signupPage.VerifyFirstNameValidationErrorAsync();
        await _signupPage.VerifyLastNameValidationErrorAsync();
        await _signupPage.VerifyEmailValidationErrorAsync();
        await _signupPage.VerifyPhoneValidationErrorAsync();
        await _signupPage.VerifyPasswordValidationErrorAsync();
        await _signupPage.VerifyConfirmPasswordValidationErrorAsync();
    }

    [Test]
    public async Task ShouldShowValidationErrorForInvalidEmail()
    {
        await _signupPage.EnterFirstNameAsync("John");
        await _signupPage.EnterLastNameAsync("Doe");
        await _signupPage.EnterEmailAsync("not-an-email");
        await _signupPage.EnterPhoneNumberAsync("07123456789");
        await _signupPage.EnterPasswordAsync("Password123!");
        await _signupPage.EnterConfirmPasswordAsync("Password123!");
        await _signupPage.ClickSignUpAsync();

        var validationMessage = Page.Locator("#email + .validation-message").First;
        await Assertions.Expect(validationMessage).ToContainTextAsync("Invalid email format");
    }

    [Test]
    public async Task ShouldShowValidationErrorForPasswordMismatch()
    {
        await _signupPage.EnterFirstNameAsync("John");
        await _signupPage.EnterLastNameAsync("Doe");
        await _signupPage.EnterEmailAsync("test@example.com");
        await _signupPage.EnterPhoneNumberAsync("07123456789");
        await _signupPage.EnterPasswordAsync("Password123!");
        await _signupPage.EnterConfirmPasswordAsync("DifferentPassword123!");
        await _signupPage.ClickSignUpAsync();

        await _signupPage.VerifyPasswordMismatchErrorAsync();
    }

    [Test]
    public async Task ShouldShowValidationErrorForInvalidPhoneNumber()
    {
        await _signupPage.EnterFirstNameAsync("John");
        await _signupPage.EnterLastNameAsync("Doe");
        await _signupPage.EnterEmailAsync("test@example.com");
        await _signupPage.EnterPhoneNumberAsync("not-a-number");
        await _signupPage.EnterPasswordAsync("Password123!");
        await _signupPage.EnterConfirmPasswordAsync("Password123!");
        await _signupPage.ClickSignUpAsync();

        var validationMessage = Page.Locator("#phoneNumber + .validation-message").First;
        await Assertions.Expect(validationMessage).ToContainTextAsync("contain 7-14 digits");
    }

    [Test]
    public async Task ShouldShowLoadingStateWhenSubmitting()
    {
        await Page.RouteAsync(new Regex("/api/auth/signup"), async route =>
        {
            await Task.Delay(500);
            await route.FulfillAsync(new()
            {
                Status = 200,
                Body = """{"succeeded":true,"message":"Account created successfully"}"""
            });
        });

        await _signupPage.SignUpAsync(
            "John",
            "Doe",
            "test@example.com",
            "07123456789",
            "Password123!",
            "Password123!"
        );

        await Expect(_signupPage.Page.Locator(".spinner-border")).ToBeVisibleAsync();
        await Expect(_signupPage.Page.Locator("button.login-button")).ToBeDisabledAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(".*/signup/verify"));
    }

    [Test]
    public async Task ShouldShowErrorMessageOnEmailAlreadyExists()
    {
        await Page.RouteAsync(new Regex("/api/auth/signup"), async route =>
        {
            await route.FulfillAsync(new()
            {
                Status = 400,
                ContentType = "application/json",
                Body = """{"succeeded":false,"message":"Registration failed","errors":["Email address is already in use"]}"""
            });
        });

        await _signupPage.SignUpAsync(
            "John",
            "Doe",
            "existing@example.com",
            "07123456789",
            "Password123!",
            "Password123!"
        );

        await Expect(_signupPage.Page.Locator(".error-title")).ToBeVisibleAsync();
        await Expect(_signupPage.Page.Locator(".error-title")).ToContainTextAsync("Registration failed");
        await Expect(_signupPage.Page.Locator(".error-details li")).ToContainTextAsync("Email address is already in use");
    }

    [Test]
    public async Task ShouldNavigateToLoginPage()
    {
        await _signupPage.ClickLoginLinkAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(".*/login"));
    }

    [Test]
    public async Task ShouldHandleUnexpectedError()
    {
        await Page.RouteAsync(new Regex("/api/auth/signup"), async route =>
        {
            await route.FulfillAsync(new()
            {
                Status = 500,
                ContentType = "application/json",
                Body = """{"succeeded":false,"message":"Sign up failed. Please review errors and try again."}"""
            });
        });

        await _signupPage.SignUpAsync(
            "John",
            "Doe",
            "test@example.com",
            "07123456789",
            "Password123!",
            "Password123!"
        );

        await Expect(_signupPage.Page.Locator(".error-title")).ToBeVisibleAsync();
        await Expect(_signupPage.Page.Locator(".error-title")).ToContainTextAsync("Sign up failed. Please review errors and try again.");
    }

    [Test]
    public async Task ShouldHandleNetworkError()
    {
        await Page.RouteAsync(new Regex("/api/auth/signup"), async route =>
        {
            await route.AbortAsync();
        });

        await _signupPage.SignUpAsync(
            "John",
            "Doe",
            "test@example.com",
            "07123456789",
            "Password123!",
            "Password123!"
        );

        await Expect(_signupPage.Page.Locator(".error-title")).ToBeVisibleAsync();
        await Expect(_signupPage.Page.Locator(".error-title")).ToContainTextAsync("An unexpected error occurred");
    }
}