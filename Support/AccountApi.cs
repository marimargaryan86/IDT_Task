using Allure.Net.Commons.Attributes;
using System.Text.Json;
using Microsoft.Playwright;

namespace IDT_Task.Support;

/// <summary>
/// Creates and deletes users through the site's API, so tests other than Register skip the UI sign-up.
/// The API answers HTTP 200 with a JSON "responseCode"; a 5xx means the overloaded server refused the call,
/// which is retried (setup only, never assertions).
/// </summary>
public class AccountApi : IAsyncDisposable
{
    private const int Attempts = 4;
    private readonly IAPIRequestContext _api;

    private AccountApi(IAPIRequestContext api) => _api = api;

    public static async Task<AccountApi> CreateAsync(IPlaywright playwright, string baseUrl) =>
        new(await playwright.APIRequest.NewContextAsync(new() { BaseURL = baseUrl }));

    [AllureStep("API: create the user")]
    public async Task CreateAccountAsync([AllureParameter(Ignore = true)] User u)
    {
        var form = _api.CreateFormData()
            .Set("name", u.Name).Set("email", u.Email).Set("password", u.Password)
            .Set("title", u.Title).Set("birth_date", u.BirthDay).Set("birth_month", u.BirthMonth).Set("birth_year", u.BirthYear)
            .Set("firstname", u.FirstName).Set("lastname", u.LastName).Set("company", u.Company)
            .Set("address1", u.Address).Set("address2", u.Address2).Set("country", u.Country)
            .Set("zipcode", u.Zipcode).Set("state", u.State).Set("city", u.City)
            .Set("mobile_number", u.Mobile);

        await SendAsync("create account", 201,
            () => _api.PostAsync(Routes.CreateAccountApi, new() { Form = form }));
    }

    /// <param name="allowMissing">
    /// True for a safety-net delete: the UI may already have deleted the account (404 "Account not found!").
    /// </param>
    [AllureStep("API: delete the user")]
    public async Task DeleteAccountAsync([AllureParameter(Ignore = true)] User u, bool allowMissing = false)
    {
        var form = _api.CreateFormData().Set("email", u.Email).Set("password", u.Password);
        await SendAsync("delete account", allowMissing ? new[] { 200, 404 } : new[] { 200 },
            () => _api.DeleteAsync(Routes.DeleteAccountApi, new() { Form = form }));
    }

    private static Task SendAsync(string what, int expectedCode, Func<Task<IAPIResponse>> call) =>
        SendAsync(what, new[] { expectedCode }, call);

    private static async Task SendAsync(string what, int[] expectedCodes, Func<Task<IAPIResponse>> call)
    {
        string last = "no response";
        for (var attempt = 1; attempt <= Attempts; attempt++)
        {
            var response = await call();
            if (response.Status >= 500)
            {
                last = $"HTTP {response.Status}";
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt)); // back off; setup only
                continue;
            }

            var body = await response.TextAsync();
            int code;
            try
            {
                using var json = JsonDocument.Parse(body);
                code = json.RootElement.GetProperty("responseCode").GetInt32();
            }
            catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
            {
                // Not the JSON the API normally returns (for example an HTML error page): say what came back.
                throw new InvalidOperationException(
                    $"API {what}: unexpected response, HTTP {response.Status}, body starts with: {Snippet(body)}", ex);
            }

            if (expectedCodes.Contains(code)) return;
            throw new InvalidOperationException($"API {what} returned responseCode {code}: {body}");
        }
        throw new InvalidOperationException($"API {what} failed after {Attempts} attempts ({last}).");
    }

    /// <summary>The first 200 characters of a response body, on one line, for error messages.</summary>
    private static string Snippet(string body)
    {
        var oneLine = string.Join(' ', body.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return oneLine.Length <= 200 ? $"\"{oneLine}\"" : $"\"{oneLine[..200]}...\"";
    }

    public async ValueTask DisposeAsync() => await _api.DisposeAsync();
}
