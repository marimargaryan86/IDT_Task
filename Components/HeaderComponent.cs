using Allure.Net.Commons.Attributes;
using IDT_Task.Pages;
using Microsoft.Playwright;

namespace IDT_Task.Components;

/// <summary>The top navigation bar, present on every page. Navigation returns the page it leads to.</summary>
public class HeaderComponent
{
    private readonly IPage _page;

    public HeaderComponent(IPage page) => _page = page;

    public ILocator LoggedInAs => _page.Locator("a:has-text('Logged in as')");

    private ILocator SignupLoginLink => _page.Locator("header a[href='/login']");
    private ILocator ProductsLink => _page.Locator("header a[href='/products']");
    private ILocator CartLink => _page.Locator("header a[href='/view_cart']");
    private ILocator DeleteAccountLink => _page.Locator("header a[href='/delete_account']");

    [AllureStep("Open Signup / Login")]
    public async Task<LoginSignupPage> ClickSignupLoginAsync()
    {
        await SignupLoginLink.ClickAsync();
        return new LoginSignupPage(_page);
    }

    [AllureStep("Open Products")]
    public async Task<ProductsPage> ClickProductsAsync()
    {
        await ProductsLink.ClickAsync();
        return new ProductsPage(_page);
    }

    [AllureStep("Open Cart")]
    public async Task<CartPage> ClickCartAsync()
    {
        await CartLink.ClickAsync();
        return new CartPage(_page);
    }

    [AllureStep("Click Delete Account")]
    public async Task<AccountStatusPage> ClickDeleteAccountAsync()
    {
        await DeleteAccountLink.ClickAsync();
        return new AccountStatusPage(_page);
    }
}
