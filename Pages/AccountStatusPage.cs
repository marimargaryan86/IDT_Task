using Allure.Net.Commons.Attributes;
using Microsoft.Playwright;

namespace IDT_Task.Pages;

/// <summary>The "Account Created!" and "Account Deleted!" confirmation pages (both end with Continue).</summary>
public class AccountStatusPage : BasePage
{
    public AccountStatusPage(IPage page) : base(page) { }

    public ILocator AccountCreated => Page.Locator("[data-qa='account-created']");
    public ILocator AccountDeleted => Page.Locator("[data-qa='account-deleted']");

    private ILocator ContinueButton => Page.Locator("[data-qa='continue-button']");

    [AllureStep("Click Continue")]
    public async Task<HomePage> ContinueAsync()
    {
        await ContinueButton.ClickAsync();
        return new HomePage(Page);
    }
}
