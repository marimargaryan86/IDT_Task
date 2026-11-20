using IDT_Task.Support;
using Microsoft.Playwright;

namespace IDT_Task.Pages;

public class PaymentPage : BasePage
{
    public PaymentPage(IPage page) : base(page) { }

    public ILocator OrderPlaced => Page.Locator("[data-qa='order-placed']");

    public async Task PayAsync()
    {
        await Page.Locator("[data-qa='name-on-card']").FillAsync(TestData.CardName);
        await Page.Locator("[data-qa='card-number']").FillAsync(TestData.CardNumber);
        await Page.Locator("[data-qa='cvc']").FillAsync(TestData.CardCvc);
        await Page.Locator("[data-qa='expiry-month']").FillAsync(TestData.CardExpiryMonth);
        await Page.Locator("[data-qa='expiry-year']").FillAsync(TestData.CardExpiryYear);
        await Page.Locator("[data-qa='pay-button']").ClickAsync();
    }
}
