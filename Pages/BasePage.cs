using Microsoft.Playwright;

namespace IDT_Task.Pages;

/// <summary>Elements shared by every page: top nav and footer. No assertions here.</summary>
public abstract class BasePage
{
    protected readonly IPage Page;

    protected BasePage(IPage page) => Page = page;

    // Header
    public ILocator LoggedInAs => Page.Locator("a:has-text('Logged in as')");
    public ILocator SignupLoginLink => Page.Locator("header a[href='/login']");
    public ILocator ProductsLink => Page.Locator("header a[href='/products']");
    public ILocator CartLink => Page.Locator("header a[href='/view_cart']");
    public ILocator DeleteAccountLink => Page.Locator("header a[href='/delete_account']");

    // Footer
    public ILocator SubscriptionHeading => Page.Locator("footer").GetByRole(AriaRole.Heading, new() { Name = "Subscription" });

    public async Task ClickSignupLoginAsync() => await SignupLoginLink.ClickAsync();
    public async Task ClickProductsAsync() => await ProductsLink.ClickAsync();
    public async Task ClickCartAsync() => await CartLink.ClickAsync();
    public async Task ClickDeleteAccountAsync() => await DeleteAccountLink.ClickAsync();

    public async Task ScrollToFooterAsync() => await Page.Locator("footer").ScrollIntoViewIfNeededAsync();

    /// <summary>Scrolls with the page's own scrolling (no 'scroll up' arrow button).</summary>
    public async Task ScrollToTopAsync() => await Page.EvaluateAsync("window.scrollTo(0, 0)");
}
