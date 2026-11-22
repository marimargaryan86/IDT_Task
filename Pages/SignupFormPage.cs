using Allure.Net.Commons.Attributes;
using IDT_Task.Support;
using Microsoft.Playwright;

namespace IDT_Task.Pages;

/// <summary>The "Enter Account Information" form.</summary>
public class SignupFormPage : BasePage
{
    public SignupFormPage(IPage page) : base(page) { }

    public ILocator EnterAccountInfoHeading => Page.GetByText("Enter Account Information");

    // Locators: data-qa where the site provides it, then id, then other selectors.
    private ILocator Password => Page.Locator("[data-qa='password']");
    private ILocator Day => Page.Locator("[data-qa='days']");
    private ILocator Month => Page.Locator("[data-qa='months']");
    private ILocator Year => Page.Locator("[data-qa='years']");
    private ILocator FirstName => Page.Locator("[data-qa='first_name']");
    private ILocator LastName => Page.Locator("[data-qa='last_name']");
    private ILocator Company => Page.Locator("[data-qa='company']");
    private ILocator Address => Page.Locator("[data-qa='address']");
    private ILocator Address2 => Page.Locator("[data-qa='address2']");
    private ILocator Country => Page.Locator("[data-qa='country']");
    private ILocator State => Page.Locator("[data-qa='state']");
    private ILocator City => Page.Locator("[data-qa='city']");
    private ILocator Zipcode => Page.Locator("[data-qa='zipcode']");
    private ILocator Mobile => Page.Locator("[data-qa='mobile_number']");
    private ILocator CreateAccountButton => Page.Locator("[data-qa='create-account']");
    private ILocator TitleRadio(string title) =>                   // the input itself has no data-qa
        Page.Locator($"input[name='title'][value='{title}']");
    private ILocator Newsletter => Page.Locator("#newsletter");   // no data-qa on the site
    private ILocator SpecialOffers => Page.Locator("#optin");     // no data-qa on the site

    [AllureStep("Fill in the account information form")]
    public async Task FillAccountAsync([AllureParameter(Ignore = true)] User u)
    {
        await TitleRadio(u.Title).CheckAsync();
        await Password.FillAsync(u.Password);
        await Day.SelectOptionAsync(u.BirthDay);
        await Month.SelectOptionAsync(u.BirthMonth);
        await Year.SelectOptionAsync(u.BirthYear);
        await Newsletter.CheckAsync();
        await SpecialOffers.CheckAsync();

        await FirstName.FillAsync(u.FirstName);
        await LastName.FillAsync(u.LastName);
        await Company.FillAsync(u.Company);
        await Address.FillAsync(u.Address);
        await Address2.FillAsync(u.Address2);
        await Country.SelectOptionAsync(u.Country);
        await State.FillAsync(u.State);
        await City.FillAsync(u.City);
        await Zipcode.FillAsync(u.Zipcode);
        await Mobile.FillAsync(u.Mobile);
    }

    [AllureStep("Submit the account form (Create Account)")]
    public async Task<AccountStatusPage> SubmitAsync()
    {
        await CreateAccountButton.ClickAsync();
        return new AccountStatusPage(Page);
    }
}
