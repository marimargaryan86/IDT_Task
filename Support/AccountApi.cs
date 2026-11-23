using Allure.Net.Commons.Attributes;
using Microsoft.Playwright;

namespace IDT_Task.Support;

/// <summary>
/// Creates and deletes users through the site's API, so tests other than Register skip the UI sign-up, and
/// every test user is removed afterwards (see <see cref="BaseTest"/>).
/// </summary>
public sealed class AccountApi : ApiClient
{
    private AccountApi(IAPIRequestContext api) : base(api) { }

    public static async Task<AccountApi> CreateAsync(IPlaywright playwright, string baseUrl) =>
        new(await NewContextAsync(playwright, baseUrl));

    [AllureStep("API: create the user")]
    public async Task CreateAccountAsync([AllureParameter(Ignore = true)] User u)
    {
        var form = Api.CreateFormData()
            .Set("name", u.Name).Set("email", u.Email).Set("password", u.Password)
            .Set("title", u.Title).Set("birth_date", u.BirthDay).Set("birth_month", u.BirthMonth).Set("birth_year", u.BirthYear)
            .Set("firstname", u.FirstName).Set("lastname", u.LastName).Set("company", u.Company)
            .Set("address1", u.Address).Set("address2", u.Address2).Set("country", u.Country)
            .Set("zipcode", u.Zipcode).Set("state", u.State).Set("city", u.City)
            .Set("mobile_number", u.Mobile);

        await SendAsync("create account", new[] { 201 },
            () => Api.PostAsync(Routes.CreateAccountApi, new() { Form = form }));
    }

    /// <param name="allowMissing">
    /// True for a safety-net delete: the UI may already have deleted the account (404 "Account not found!").
    /// </param>
    [AllureStep("API: delete the user")]
    public async Task DeleteAccountAsync([AllureParameter(Ignore = true)] User u, bool allowMissing = false)
    {
        var form = Api.CreateFormData().Set("email", u.Email).Set("password", u.Password);
        await SendAsync("delete account", allowMissing ? new[] { 200, 404 } : new[] { 200 },
            () => Api.DeleteAsync(Routes.DeleteAccountApi, new() { Form = form }));
    }
}
