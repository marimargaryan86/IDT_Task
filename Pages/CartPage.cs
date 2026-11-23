using Allure.Net.Commons.Attributes;
using Microsoft.Playwright;

namespace IDT_Task.Pages;

public class CartPage : BasePage
{
    public CartPage(IPage page) : base(page) { }

    public ILocator ShoppingCartBreadcrumb =>
        Page.Locator(".breadcrumbs li.active", new() { HasText = "Shopping Cart" });
    public ILocator CartProductNames => Page.Locator("#cart_info_table td.cart_description h4 a");
    public ILocator CartRows => Page.Locator("#cart_info_table tbody tr");

    // The site has no data-qa on the checkout button, and it is an <a> without href, so it has no link role
    // (a role-based locator would not find it). Found by its class, the one the site's own script uses.
    private ILocator ProceedToCheckoutButton =>
        Page.Locator("a.check_out", new() { HasText = "Proceed To Checkout" });

    [AllureStep("Proceed to checkout")]
    public async Task<CheckoutPage> ProceedToCheckoutAsync()
    {
        await ProceedToCheckoutButton.ClickAsync();
        return new CheckoutPage(Page);
    }
}
