using Allure.Net.Commons.Attributes;
using Microsoft.Playwright;

namespace IDT_Task.Pages;

public class CheckoutPage : BasePage
{
    public CheckoutPage(IPage page) : base(page) { }

    public ILocator AddressDetailsHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "Address Details" });
    public ILocator ReviewOrderHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "Review Your Order" });

    // The site has no data-qa on these two.
    private ILocator CommentBox => Page.Locator("textarea[name='message']");
    private ILocator PlaceOrderLink => Page.GetByRole(AriaRole.Link, new() { Name = "Place Order" });

    [AllureStep("Enter an order comment")]
    public async Task EnterCommentAsync(string text) => await CommentBox.FillAsync(text);

    [AllureStep("Place the order")]
    public async Task<PaymentPage> PlaceOrderAsync()
    {
        await PlaceOrderLink.ClickAsync();
        return new PaymentPage(Page);
    }
}
