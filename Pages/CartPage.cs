using Microsoft.Playwright;

namespace IDT_Task.Pages;

public class CartPage : BasePage
{
    public CartPage(IPage page) : base(page) { }

    public ILocator CartRows => Page.Locator("#cart_info_table tbody tr");

    public async Task ProceedToCheckoutAsync() =>
        await Page.GetByText("Proceed To Checkout").ClickAsync();
}
