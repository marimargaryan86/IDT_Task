using Microsoft.Playwright;

namespace IDT_Task.Pages;

public class CheckoutPage : BasePage
{
    public CheckoutPage(IPage page) : base(page) { }

    public ILocator AddressDetailsHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "Address Details" });
    public ILocator ReviewOrderHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "Review Your Order" });

    public async Task EnterCommentAsync(string text) =>
        await Page.Locator("textarea[name='message']").FillAsync(text);

    public async Task PlaceOrderAsync() =>
        await Page.GetByRole(AriaRole.Link, new() { Name = "Place Order" }).ClickAsync();
}
