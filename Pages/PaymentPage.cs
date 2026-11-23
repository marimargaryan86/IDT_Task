using Allure.Net.Commons.Attributes;
using IDT_Task.Support;
using Microsoft.Playwright;

namespace IDT_Task.Pages;

public class PaymentPage : BasePage
{
    public PaymentPage(IPage page) : base(page) { }

    public ILocator OrderPlaced => Page.Locator("[data-qa='order-placed']");
    public ILocator OrderConfirmation => Page.GetByText("Congratulations! Your order has been confirmed!");

    private ILocator NameOnCard => Page.Locator("[data-qa='name-on-card']");
    private ILocator CardNumber => Page.Locator("[data-qa='card-number']");
    private ILocator Cvc => Page.Locator("[data-qa='cvc']");
    private ILocator ExpiryMonth => Page.Locator("[data-qa='expiry-month']");
    private ILocator ExpiryYear => Page.Locator("[data-qa='expiry-year']");
    private ILocator PayButton => Page.Locator("[data-qa='pay-button']");

    [AllureStep("Pay with the test card")]
    public async Task PayAsync()
    {
        await NameOnCard.FillAsync(TestData.CardName);
        await CardNumber.FillAsync(TestData.CardNumber);
        await Cvc.FillAsync(TestData.CardCvc);
        await ExpiryMonth.FillAsync(TestData.CardExpiryMonth);
        await ExpiryYear.FillAsync(TestData.CardExpiryYear);
        await PayButton.ClickAsync();
    }
}
