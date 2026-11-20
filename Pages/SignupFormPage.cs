using IDT_Task.Support;
using Microsoft.Playwright;

namespace IDT_Task.Pages;

/// <summary>Account information form, account-created and account-deleted confirmations.</summary>
public class SignupFormPage : BasePage
{
    public SignupFormPage(IPage page) : base(page) { }

    public ILocator EnterAccountInfoHeading => Page.GetByText("Enter Account Information");
    public ILocator AccountCreated => Page.Locator("[data-qa='account-created']");
    public ILocator AccountDeleted => Page.Locator("[data-qa='account-deleted']");

    public async Task FillAccountAsync(User u)
    {
        await Page.Locator("#id_gender1").CheckAsync();
        await Page.Locator("[data-qa='password']").FillAsync(u.Password);
        await Page.Locator("[data-qa='days']").SelectOptionAsync("10");
        await Page.Locator("[data-qa='months']").SelectOptionAsync("May");
        await Page.Locator("[data-qa='years']").SelectOptionAsync("1990");
        await Page.Locator("#newsletter").CheckAsync();
        await Page.Locator("#optin").CheckAsync();

        await Page.Locator("[data-qa='first_name']").FillAsync(u.FirstName);
        await Page.Locator("[data-qa='last_name']").FillAsync(u.LastName);
        await Page.Locator("[data-qa='company']").FillAsync(u.Company);
        await Page.Locator("[data-qa='address']").FillAsync(u.Address);
        await Page.Locator("[data-qa='address2']").FillAsync(u.Address2);
        await Page.Locator("[data-qa='country']").SelectOptionAsync(u.Country);
        await Page.Locator("[data-qa='state']").FillAsync(u.State);
        await Page.Locator("[data-qa='city']").FillAsync(u.City);
        await Page.Locator("[data-qa='zipcode']").FillAsync(u.Zipcode);
        await Page.Locator("[data-qa='mobile_number']").FillAsync(u.Mobile);
    }

    public async Task SubmitAsync() => await Page.Locator("[data-qa='create-account']").ClickAsync();

    public async Task ContinueAsync() => await Page.Locator("[data-qa='continue-button']").ClickAsync();
}
