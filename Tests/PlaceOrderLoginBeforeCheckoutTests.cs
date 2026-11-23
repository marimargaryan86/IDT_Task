using Allure.Net.Commons.Attributes;
using IDT_Task.Pages;
using IDT_Task.Support;

namespace IDT_Task.Tests;

[TestFixture]
[AllureFeature("Checkout")]
public class PlaceOrderLoginBeforeCheckoutTests : BaseTest
{
    [Test]
    [AllureName("Test Case 16: Place Order: Login before Checkout")]
    [Description("automationexercise.com Test Case 16")]
    public async Task TC16_PlaceOrder_LoginBeforeCheckout()
    {
        // Precondition: an existing account, created through the API (not the sign-up UI). The case deletes it
        // in the UI at the end; BaseTest deletes it afterwards if the case did not get that far.
        var user = await CreateTestUserViaApiAsync();

        // Login before checkout
        var home = await new HomePage(Page).OpenAsync();
        await Expect(home.HeroHeading).ToBeVisibleAsync();

        var login = await home.Header.ClickSignupLoginAsync();
        await Expect(login.LoginHeading).ToBeVisibleAsync();

        home = await login.LoginAsync(user.Email, user.Password);
        await Expect(home.Header.LoggedInAs).ToContainTextAsync(user.Name);

        // Add products, then open the cart from the header
        var products = await home.Header.ClickProductsAsync();
        var added = await products.AddProductsToCartAsync(TestData.ProductsToOrder);
        var cart = await products.Header.ClickCartAsync();
        await Expect(cart.ShoppingCartBreadcrumb).ToBeVisibleAsync();
        await Expect(cart.CartRows).ToHaveCountAsync(TestData.ProductsToOrder);
        await Expect(cart.CartProductNames).ToHaveTextAsync(added); // the products we added

        // Checkout
        var checkout = await cart.ProceedToCheckoutAsync();
        await Expect(checkout.AddressDetailsHeading).ToBeVisibleAsync();
        await Expect(checkout.ReviewOrderHeading).ToBeVisibleAsync();
        await checkout.EnterCommentAsync(TestData.OrderComment);

        // Pay
        var payment = await checkout.PlaceOrderAsync();
        await payment.PayAsync();
        await Expect(payment.OrderPlaced).ToBeVisibleAsync();
        await Expect(payment.OrderConfirmation).ToBeVisibleAsync();

        // Delete the account through the UI
        var deleted = await payment.Header.ClickDeleteAccountAsync();
        await Expect(deleted.AccountDeleted).ToBeVisibleAsync();
        await deleted.ContinueAsync();
    }
}
