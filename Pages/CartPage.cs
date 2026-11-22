using Allure.Net.Commons.Attributes;
using Microsoft.Playwright;

namespace IDT_Task.Pages;

public class CartPage : BasePage
{
    public CartPage(IPage page) : base(page) { }

    public ILocator ShoppingCartBreadcrumb =>
        Page.Locator(".breadcrumbs li.active", new() { HasText = "Shopping Cart" });
    public ILocator CartRows => Page.Locator("#cart_info_table tbody tr");

    // The site has no data-qa on the checkout button.
    private ILocator ProceedToCheckoutButton => Page.GetByText("Proceed To Checkout");

    [AllureStep("Proceed to checkout")]
    public async Task<CheckoutPage> ProceedToCheckoutAsync()
    {
        await ProceedToCheckoutButton.ClickAsync();
        return new CheckoutPage(Page);
    }
}
