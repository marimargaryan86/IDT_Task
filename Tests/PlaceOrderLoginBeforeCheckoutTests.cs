using IDT_Task.Pages;
using IDT_Task.Support;

namespace IDT_Task.Tests;

[TestClass]
public class PlaceOrderLoginBeforeCheckoutTests : BaseTest
{
    [TestMethod]
    public async Task PlaceOrder_LoginBeforeCheckout()
    {
        var user = TestData.RandomUser();
        var home = new HomePage(Page);
        var login = new LoginSignupPage(Page);
        var products = new ProductsPage(Page);
        var cart = new CartPage(Page);
        var checkout = new CheckoutPage(Page);
        var payment = new PaymentPage(Page);
        var form = new SignupFormPage(Page);

        // Precondition: an existing account, then log out so the test really logs in.
        await AccountFlow.RegisterAsync(Page, user);
        await AccountFlow.LogoutAsync(Page);

        // Login before checkout
        await home.ClickSignupLoginAsync();
        await Expect(login.LoginHeading).ToBeVisibleAsync();
        await login.LoginAsync(user.Email, user.Password);
        await Expect(home.LoggedInAs).ToContainTextAsync(user.Name);

        // Add product and open the cart
        await home.ClickProductsAsync();
        await products.AddFirstProductToCartAsync();
        await products.ViewCartFromModalAsync();
        await Expect(cart.CartRows).ToHaveCountAsync(1);

        // Checkout
        await cart.ProceedToCheckoutAsync();
        await Expect(checkout.AddressDetailsHeading).ToBeVisibleAsync();
        await Expect(checkout.ReviewOrderHeading).ToBeVisibleAsync();
        await checkout.EnterCommentAsync("Please deliver in the morning.");
        await checkout.PlaceOrderAsync();

        // Pay
        await payment.PayAsync();
        await Expect(payment.OrderPlaced).ToBeVisibleAsync();

        // Cleanup: delete the account
        await home.ClickDeleteAccountAsync();
        await Expect(form.AccountDeleted).ToBeVisibleAsync();
        await form.ContinueAsync();
    }
}
