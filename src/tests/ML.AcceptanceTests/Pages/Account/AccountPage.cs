namespace ML.AcceptanceTests.Pages.Account;
internal class AccountPage(IBrowser browser, IPage page) : BasePage
{
    public override IBrowser Browser { get; } = browser;

    public override IPage Page { get; set; } = page;
    public override string PagePath => $"{BaseUrl}/account";
}
