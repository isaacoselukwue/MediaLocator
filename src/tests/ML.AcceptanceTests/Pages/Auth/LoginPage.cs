namespace ML.AcceptanceTests.Pages.Auth;
public class LoginPage(IBrowser browser, IPage page) : BasePage
{
    public override IBrowser Browser { get; } = browser;

    public override IPage Page { get; set; } = page;
    public override string PagePath => $"{BaseUrl}/login";
    private ILocator EmailInput => Page.Locator("#email");
    private ILocator PasswordInput => Page.Locator("#password");
    private ILocator LoginButton => Page.Locator("button.login-button");
    private ILocator ErrorContainer => Page.Locator(".error-container");
    private ILocator ErrorTitle => Page.Locator(".error-title");
    private ILocator ErrorDetails => Page.Locator(".error-details li");
    private ILocator ForgotPasswordLink => Page.Locator("a[href='forgot-password']");
    private ILocator SignUpLink => Page.Locator("a[href='signup']");
    private ILocator GoogleLoginButton => Page.Locator(".social-button >> text=Google");
    private ILocator TwitterLoginButton => Page.Locator(".social-button >> text=Twitter");
    private ILocator LoadingIndicator => Page.Locator(".spinner-border");

    public async Task NavigateToAsync()
    {
        await Page.GotoAsync(PagePath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task EnterEmailAsync(string email)
    {
        await EmailInput.FillAsync(email);
    }

    public async Task EnterPasswordAsync(string password)
    {
        await PasswordInput.FillAsync(password);
    }

    public async Task ClickLoginAsync()
    {
        await LoginButton.ClickAsync();
    }

    public async Task ClickForgotPasswordAsync()
    {
        await ForgotPasswordLink.ClickAsync();
    }

    public async Task ClickSignUpAsync()
    {
        await SignUpLink.ClickAsync();
    }

    public async Task ClickGoogleLoginAsync()
    {
        await GoogleLoginButton.ClickAsync();
    }

    public async Task ClickTwitterLoginAsync()
    {
        await TwitterLoginButton.ClickAsync();
    }

    public async Task LoginAsync(string email, string password)
    {
        await EnterEmailAsync(email);
        await EnterPasswordAsync(password);
        await ClickLoginAsync();
    }

    public async Task<bool> IsErrorContainerVisibleAsync()
    {
        return await ErrorContainer.IsVisibleAsync();
    }

    public async Task<string> GetErrorTitleTextAsync()
    {
        return await ErrorTitle.TextContentAsync() ?? string.Empty;
    }

    public async Task<string[]> GetErrorDetailsAsync()
    {
        var details = await ErrorDetails.AllTextContentsAsync();
        return details.ToArray();
    }

    public async Task<bool> IsLoadingIndicatorVisibleAsync()
    {
        return await LoadingIndicator.IsVisibleAsync();
    }

    public async Task<bool> IsLoginButtonDisabledAsync()
    {
        return await LoginButton.IsDisabledAsync();
    }

    public async Task VerifyEmailValidationErrorAsync()
    {
        var validationMessageLocator = Page.Locator(".validation-message").First;
        var validationMessage = await validationMessageLocator.TextContentAsync();
        await Assertions.Expect(validationMessageLocator).ToContainTextAsync("Email is required");
    }

    public async Task VerifyPasswordValidationErrorAsync()
    {
        var validationMessageLocator = Page.Locator(".validation-message").Nth(1).First;
        var validationMessage = await Page.Locator(".validation-message").Nth(1).TextContentAsync();
        await Assertions.Expect(validationMessageLocator).ToContainTextAsync("Password is required");
    }
}
