namespace ML.AcceptanceTests.Pages.Auth;
public class SignupPage(IBrowser browser, IPage page) : BasePage
{
    public override string PagePath => $"{BaseUrl}/signup";

    public override IBrowser Browser { get; } = browser;

    public override IPage Page { get; set; } = page;
    private ILocator FirstNameInput => Page.Locator("#firstName");
    private ILocator LastNameInput => Page.Locator("#lastName");
    private ILocator EmailInput => Page.Locator("#email");
    private ILocator PhoneNumberInput => Page.Locator("#phoneNumber");
    private ILocator PasswordInput => Page.Locator("#password");
    private ILocator ConfirmPasswordInput => Page.Locator("#confirmPassword");
    private ILocator SignUpButton => Page.Locator("button.login-button");
    private ILocator ErrorContainer => Page.Locator(".error-container");
    private ILocator ErrorTitle => Page.Locator(".error-title");
    private ILocator ErrorDetails => Page.Locator(".error-details li");
    private ILocator GoogleSignupButton => Page.Locator(".social-button >> text=Google");
    private ILocator TwitterSignupButton => Page.Locator(".social-button >> text=Twitter");
    private ILocator LoginLink => Page.Locator("a[href='login']");
    private ILocator LoadingIndicator => Page.Locator(".spinner-border");

    public async Task NavigateToAsync()
    {
        await Page.GotoAsync(PagePath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task EnterFirstNameAsync(string firstName)
    {
        await FirstNameInput.FillAsync(firstName);
    }

    public async Task EnterLastNameAsync(string lastName)
    {
        await LastNameInput.FillAsync(lastName);
    }

    public async Task EnterEmailAsync(string email)
    {
        await EmailInput.FillAsync(email);
    }

    public async Task EnterPhoneNumberAsync(string phoneNumber)
    {
        await PhoneNumberInput.FillAsync(phoneNumber);
    }

    public async Task EnterPasswordAsync(string password)
    {
        await PasswordInput.FillAsync(password);
    }

    public async Task EnterConfirmPasswordAsync(string confirmPassword)
    {
        await ConfirmPasswordInput.FillAsync(confirmPassword);
    }

    public async Task ClickSignUpAsync()
    {
        await SignUpButton.ClickAsync();
    }

    public async Task ClickGoogleSignupAsync()
    {
        await GoogleSignupButton.ClickAsync();
    }

    public async Task ClickTwitterSignupAsync()
    {
        await TwitterSignupButton.ClickAsync();
    }

    public async Task ClickLoginLinkAsync()
    {
        await LoginLink.ClickAsync();
    }

    public async Task SignUpAsync(string firstName, string lastName, string email, string phoneNumber, string password, string confirmPassword)
    {
        await EnterFirstNameAsync(firstName);
        await EnterLastNameAsync(lastName);
        await EnterEmailAsync(email);
        await EnterPhoneNumberAsync(phoneNumber);
        await EnterPasswordAsync(password);
        await EnterConfirmPasswordAsync(confirmPassword);
        await ClickSignUpAsync();
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

    public async Task<bool> IsSignUpButtonDisabledAsync()
    {
        return await SignUpButton.IsDisabledAsync();
    }

    public async Task VerifyFirstNameValidationErrorAsync()
    {
        var validationMessageLocator = Page.Locator("#firstName + .validation-message").First;
        await Assertions.Expect(validationMessageLocator).ToContainTextAsync("First name is required");
    }

    public async Task VerifyLastNameValidationErrorAsync()
    {
        var validationMessageLocator = Page.Locator("#lastName + .validation-message").First;
        await Assertions.Expect(validationMessageLocator).ToContainTextAsync("Last name is required");
    }

    public async Task VerifyEmailValidationErrorAsync()
    {
        var validationMessage = Page.Locator("#email + .validation-message").First;
        await Assertions.Expect(validationMessage).ToContainTextAsync("Email is required");
    }

    public async Task VerifyPhoneValidationErrorAsync()
    {
        var validationMessage = Page.Locator("#phoneNumber + .validation-message").First;
        await Assertions.Expect(validationMessage).ToContainTextAsync("Phone number is required");
    }

    public async Task VerifyPasswordValidationErrorAsync()
    {
        var validationMessage = Page.Locator("#password + .validation-message").First;
        await Assertions.Expect(validationMessage).ToContainTextAsync("Password is required");
    }

    public async Task VerifyConfirmPasswordValidationErrorAsync()
    {
        var validationMessage = Page.Locator("#confirmPassword + .validation-message").First;
        await Assertions.Expect(validationMessage).ToContainTextAsync("Please confirm your password");
    }

    public async Task VerifyPasswordMismatchErrorAsync()
    {
        var validationMessage = Page.Locator("#confirmPassword + .validation-message").First;
        await Assertions.Expect(validationMessage).ToContainTextAsync("Passwords do not match");
    }
}